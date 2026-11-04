// Each link's mass, centre and inertia are data on the body model (Luis, November 3: "the weights where the models
// have them"): the factory's shapes give b3.fk's centres (the fk test checks them against the prototype); here, the
// shapes' inertias by their formulas, and a measured centre or mass set on one link moves the body's centre and the
// joints' moments exactly as the statics say it should.
using System;
using System.Linq;

namespace WonderGather.AnyBody.Tests
{
    public static class MassDataTests
    {
        public static void Run()
        {
            Console.WriteLine("== mass: each link's mass, centre and inertia as data (shapes; a measured centre fed in)");
            // the shape formulas: a capsule with no cylinder is a sphere; a thin capsule a rod; a cube's box
            var sph = BodyFactory.Capsule(2.0, 0.1, 0.0, 2);
            Check.Near(sph.M00, 0.4 * 2.0 * 0.01, 1e-12, "capsule of length 0 = sphere (x)");
            Check.Near(sph.M22, 0.4 * 2.0 * 0.01, 1e-12, "capsule of length 0 = sphere (z)");
            var rod = BodyFactory.Capsule(3.0, 1e-6, 1.0, 2);
            Check.Near(rod.M00, 3.0 / 12.0, 1e-5, "thin capsule = rod (transverse)");
            Check.That(rod.M22 < 1e-11, "thin capsule = rod (axial ~ 0)");
            var cube = BodyFactory.Box(6.0, 1.0, 1.0, 1.0);
            Check.Near(cube.M11, 1.0, 1e-12, "unit cube of 6 kg: I = 1");
            var hipsCap = BodyFactory.Capsule(1.0, 0.1, 0.4, 1);
            Check.That(hipsCap.M11 < hipsCap.M00 && Math.Abs(hipsCap.M00 - hipsCap.M22) < 1e-15, "capsule along y: the small moment about y");

            // every prototype body: centres and inertias filled, inertias positive, the total mass the sum of the links'
            int bodies = 0;
            foreach (var v in BodyFactory.PrototypeBodies)
            {
                var b = BodyFactory.Make(v);
                bool ok = true;
                for (int l = 0; l < BodyLinks.Count; l++)
                {
                    var I = b.LinkInertia[l];
                    ok &= I.M00 > 0 && I.M11 > 0 && I.M22 > 0;
                    // triangle inequality of a real body's principal moments
                    ok &= I.M00 + I.M11 >= I.M22 * (1 - 1e-12) && I.M11 + I.M22 >= I.M00 * (1 - 1e-12) && I.M00 + I.M22 >= I.M11 * (1 - 1e-12);
                }
                Check.That(ok, $"{v.Name}: each link's inertia positive and physical");
                Check.Near(b.Mass, Enumerable.Range(0, BodyLinks.Count).Sum(l => b.MassOf(l)), 1e-12, $"{v.Name}: M = sum of the links' masses");
                bodies++;
            }

            // a measured centre and mass fed into one link: the body's centre and every joint's subtree moment follow
            var cs = Ref.Cases()[0];
            var b0 = ExactTests.Body(cs.Body);
            var st = StanceLibrary.Make(b0, cs.Family, cs.Args);
            var p0 = Statics.Build(b0, cs.Q, st.Supports);
            var b1 = b0.Clone();
            var dc = new Vec3(0.04, -0.01, 0.03); // in the trunk's own frame
            double dm = 1.5;
            b1.SetLinkMassProperties(BodyLinks.Trunk, b0.MassTrunk + dm, b0.LinkCentre[BodyLinks.Trunk] + dc, b0.LinkInertia[BodyLinks.Trunk]);
            Check.Near(b1.Mass, b0.Mass + dm, 1e-12, "a measured trunk mass changes M by as much");
            Check.That(b0.LinkCentre[BodyLinks.Trunk].Z == b0.TrunkCentre && b0.MassTrunk != b1.MassTrunk, "Clone keeps the original's data");
            var p1 = Statics.Build(b1, cs.Q, st.Supports);
            // expected: S1 = S0 + dm * c0 + (m0 + dm) * RT dc, with c0 the trunk's old centre in the world
            var k = new Kin();
            var qd = cs.Q.Select(x => new D(x)).ToArray();
            Kinematics.Forward(b0, qd, k);
            var c0 = k.LinkCom[BodyLinks.Trunk].V;
            var RT = k.R[BodyLinks.Trunk];
            var rdc = new Vec3(RT.Mul(dc).X.V, RT.Mul(dc).Y.V, RT.Mul(dc).Z.V);
            var S0 = b0.Mass * p0.Com;
            var S1want = S0 + dm * c0 + (b0.MassTrunk + dm) * rdc;
            var S1 = b1.Mass * p1.Com;
            Check.That((S1 - S1want).Length < 1e-10, $"the body's centre moves with the measured trunk centre ({(S1 - S1want).Length:E1})");
            // the back joint's subtree holds the trunk: its moment moves by the same; the knees' do not
            var back0 = p0.Joints[0];
            var back1 = p1.Joints[0];
            Check.That((back1.SubMoment - (back0.SubMoment + dm * c0 + (b0.MassTrunk + dm) * rdc)).Length < 1e-10 &&
                       Math.Abs(back1.SubMass - back0.SubMass - dm) < 1e-12, "the back's subtree mass and moment follow");
            int kneeL = Array.FindIndex(p0.Joints, j => j.Name == "kneeL");
            Check.That((p1.Joints[kneeL].SubMoment - p0.Joints[kneeL].SubMoment).Length == 0.0, "the knee's subtree does not change");
            Console.WriteLine($"   {bodies} bodies: inertias physical, M the links' sum; a measured trunk centre and mass move the body's centre and the back's moment as the statics say");
        }
    }
}
