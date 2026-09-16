/*
 * Gedcom Exporter - a utility for exporting a Sims 2 'hood genealogy as GEDCOM
 *
 * Built on Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files, by William Howard
 *   https://github.com/whoward69/Sims2Tools - reuse permitted, see Code Reuse Policy
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.DBPF;
using Sims2Tools.DBPF.Neighbourhood;
using System.Collections.Generic;

namespace Sims2Tools.GedcomExporter
{
    /// <summary>One synthesized GEDCOM FAM unit. TS2 has no family record - this is built
    /// entirely from FAMt ties.</summary>
    public class Family
    {
        public TypeGUID? Husband { get; set; }
        public TypeGUID? Wife { get; set; }
        public List<TypeGUID> Children { get; } = new List<TypeGUID>();
        public bool IsMarried { get; set; }

        /// <summary>True for a sibling-only phantom family: no HUSB/WIFE, only CHIL, emitted
        /// so that sibling ties with no recorded parents survive the round trip.</summary>
        public bool IsPhantomSiblingFamily { get; set; }
    }

    /// <summary>
    /// Builds synthesized FAM units from a TieGraph. Operates on the full, unfiltered Sim/tie
    /// universe - ExportOptions filtering (deceased/pets/NPCs) is applied later, when the
    /// families are rendered, not here, so a filtered-out parent doesn't silently break a family.
    /// </summary>
    public class FamilyBuilder
    {
        private readonly struct ParentKey
        {
            public readonly TypeGUID? Father;
            public readonly TypeGUID? Mother;

            public ParentKey(TypeGUID? father, TypeGUID? mother)
            {
                Father = father;
                Mother = mother;
            }

            public override bool Equals(object obj) =>
                obj is ParentKey other && Father.Equals(other.Father) && Mother.Equals(other.Mother);

            public override int GetHashCode() => (Father, Mother).GetHashCode();
        }

        public List<Family> Build(IReadOnlyDictionary<TypeGUID, SimRecord> sims, TieGraph ties, ExportReport report)
        {
            List<Family> families = new List<Family>();
            HashSet<TypeGUID> assignedAsChild = new HashSet<TypeGUID>();
            Dictionary<ParentKey, Family> byParentKey = new Dictionary<ParentKey, Family>();

            // 1: group every Sim with at least one recorded parent by (father, mother) pair.
            // The tie type (MyFatherIs/MyMotherIs) does not always match the parent's recorded
            // Gender - TS2 hoods played with male-pregnancy mods or hand-edited ties can record
            // a father-tie target whose SDSC says Female, or vice versa. HUSB/WIFE is assigned by
            // actual Gender when it disambiguates; the tie-type mapping is only the fallback.
            foreach (TypeGUID guid in sims.Keys)
            {
                TypeGUID? father = ties.Father(guid);
                TypeGUID? mother = ties.Mother(guid);
                if (father == null && mother == null) continue;

                ParentKey key = new ParentKey(father, mother);
                if (!byParentKey.TryGetValue(key, out Family family))
                {
                    (TypeGUID? husb, TypeGUID? wife) = AssignParentRoles(father, mother, sims, report);
                    family = new Family { Husband = husb, Wife = wife };
                    byParentKey[key] = family;
                    families.Add(family);
                }

                family.Children.Add(guid);
                assignedAsChild.Add(guid);
            }

            // 2: mark IsMarried on any family whose parents also have a reciprocal spouse tie,
            //    and track every couple already covered so step 3 doesn't duplicate them.
            HashSet<(TypeGUID, TypeGUID)> coveredCouples = new HashSet<(TypeGUID, TypeGUID)>();
            foreach (Family family in families)
            {
                if (family.Husband.HasValue && family.Wife.HasValue)
                {
                    (TypeGUID, TypeGUID) couple = Canonical(family.Husband.Value, family.Wife.Value);
                    coveredCouples.Add(couple);

                    foreach (TypeGUID spouse in ties.Spouses(family.Husband.Value))
                    {
                        if (spouse.Equals(family.Wife.Value)) { family.IsMarried = true; break; }
                    }
                }
            }

            // 3: childless couples still need a FAM so they render as a couple at all.
            HashSet<(TypeGUID, TypeGUID)> seenSpousePairs = new HashSet<(TypeGUID, TypeGUID)>();
            foreach (TieEdge edge in ties.Edges)
            {
                if (edge.Type != FamilyTieTypes.ImMarriedTo) continue;

                (TypeGUID, TypeGUID) couple = Canonical(edge.From, edge.To);
                if (!seenSpousePairs.Add(couple)) continue;
                if (coveredCouples.Contains(couple)) continue;

                (TypeGUID husb, TypeGUID wife) = AssignRoles(edge.From, edge.To, sims);

                families.Add(new Family { Husband = husb, Wife = wife, IsMarried = true });
            }

            // 4: sibling-only phantom families - Sims with no recorded parents, linked only by
            //    MySiblingIs. Connected components via union-find over the sibling edges.
            Dictionary<TypeGUID, TypeGUID> parent = new Dictionary<TypeGUID, TypeGUID>();
            TypeGUID Find(TypeGUID x)
            {
                if (!parent.ContainsKey(x)) parent[x] = x;
                while (!parent[x].Equals(x)) { x = parent[x]; }
                return x;
            }
            void Union(TypeGUID a, TypeGUID b)
            {
                TypeGUID ra = Find(a), rb = Find(b);
                if (!ra.Equals(rb)) parent[ra] = rb;
            }

            foreach (TypeGUID guid in sims.Keys)
            {
                if (assignedAsChild.Contains(guid)) continue; // has a recorded parent already
                foreach (TypeGUID sibling in ties.Siblings(guid))
                {
                    if (assignedAsChild.Contains(sibling)) continue;
                    Union(guid, sibling);
                }
            }

            Dictionary<TypeGUID, List<TypeGUID>> components = new Dictionary<TypeGUID, List<TypeGUID>>();
            foreach (TypeGUID guid in parent.Keys)
            {
                TypeGUID root = Find(guid);
                if (!components.TryGetValue(root, out List<TypeGUID> members))
                {
                    members = new List<TypeGUID>();
                    components[root] = members;
                }
                members.Add(guid);
            }

            foreach (List<TypeGUID> members in components.Values)
            {
                if (members.Count < 2) continue; // a lone Sim with a dangling/self sibling tie - not a family

                Family phantom = new Family { IsPhantomSiblingFamily = true };
                phantom.Children.AddRange(members);
                families.Add(phantom);
            }

            report.FamiliesWritten = families.Count;
            return families;
        }

        private static (TypeGUID? husb, TypeGUID? wife) AssignParentRoles(
            TypeGUID? father, TypeGUID? mother, IReadOnlyDictionary<TypeGUID, SimRecord> sims, ExportReport report)
        {
            if (father.HasValue && mother.HasValue)
            {
                bool fatherIsMale = sims.TryGetValue(father.Value, out SimRecord fs) && fs.Gender == Gender.Male;
                bool motherIsMale = sims.TryGetValue(mother.Value, out SimRecord ms) && ms.Gender == Gender.Male;

                if (motherIsMale && !fatherIsMale)
                {
                    // The MyMotherIs tie points at a Sim recorded as Male - swap roles so HUSB/WIFE
                    // match actual Gender rather than the raw tie type.
                    report.Warn($"{mother.Value} is tied as MyMotherIs to a child but is recorded Male; " +
                                $"{father.Value} is tied as MyFatherIs but is recorded Female. Assigned by Gender instead of tie type.");
                    return (mother, father);
                }

                if (fatherIsMale && motherIsMale)
                {
                    report.Warn($"Both parents ({father.Value}, {mother.Value}) are recorded Male; kept HUSB/WIFE by tie type.");
                }
            }

            return (father, mother);
        }

        private static (TypeGUID, TypeGUID) Canonical(TypeGUID a, TypeGUID b) =>
            a.CompareTo(b) <= 0 ? (a, b) : (b, a);

        private static (TypeGUID husb, TypeGUID wife) AssignRoles(TypeGUID a, TypeGUID b, IReadOnlyDictionary<TypeGUID, SimRecord> sims)
        {
            bool aIsMale = sims.TryGetValue(a, out SimRecord simA) && simA.Gender == Gender.Male;
            bool bIsMale = sims.TryGetValue(b, out SimRecord simB) && simB.Gender == Gender.Male;

            if (aIsMale && !bIsMale) return (a, b);
            if (bIsMale && !aIsMale) return (b, a);

            // same-sex or unknown gender: stable arbitrary assignment, matches the source plan's
            // guidance that HUSB/WIFE tag choice for same-sex couples is cosmetic only.
            return (a, b);
        }
    }
}
