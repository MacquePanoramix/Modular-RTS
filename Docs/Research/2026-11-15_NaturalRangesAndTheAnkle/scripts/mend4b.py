import sys
sys.stdout.reconfigure(encoding='utf-8')
exec(open('mend4.py',encoding='utf-8').read().split('print("# 1.')[0])
N=400000
th=rng.uniform(0,180,N); own=angle_axis(th,rng.normal(size=(N,3))); d=rng.uniform(-20,20,size=(N,3))
eb=turn_between(A(own,d),B(own,d)); ec=turn_between(A(own,d),C(own,d))
print("max |eb-ec| %.3f  mean eb %.3f mean ec %.3f"%(np.abs(eb-ec).max(),eb.mean(),ec.mean()))
m=th<60
print("theta<60: mean eb %.3f ec %.3f ; frac b nearer %.3f ; mean(eb-ec) %.4f"%(eb[m].mean(),ec[m].mean(),(eb[m]<ec[m]).mean(),(eb-ec)[m].mean()))
# within one own, over d sign: is (c)-(a) at d equal (b)-(a) at some reflected d?
