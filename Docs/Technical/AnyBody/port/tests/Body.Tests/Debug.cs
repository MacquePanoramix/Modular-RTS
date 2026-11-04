using System;
using System.Linq;
namespace WonderGather.AnyBody.Tests
{
    public static class DebugRun
    {
        public static void Fk0()
        {
            var cs = Ref.Cases()[0];
            var b = ExactTests.Body(cs.Body);
            var k = new Kin();
            var qd = cs.Q.Select(v => new D(v)).ToArray();
            Kinematics.Forward(b, qd, k);
            foreach (var n in new[] { "P", "waist", "neck", "shL", "elL", "wrL", "hjL", "knL", "anL", "ballL" })
                Console.WriteLine(n + " " + k[Kin.Point(n)].V);
            Console.WriteLine("Rhips " + k.R[BodyLinks.Hips].V);
            Console.WriteLine("Rtrunk " + k.R[BodyLinks.Trunk].V);
        }
    }
}
