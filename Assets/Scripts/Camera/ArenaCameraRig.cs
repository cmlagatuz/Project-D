// Project D | Codename: Zantetsuken — ArenaCameraRig.cs (GCD 10.1)
using UnityEngine;
namespace ProjectD.Gameplay.CameraRig
{
    public sealed class ArenaCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float height = 14f, distance = 10f, pitchDegrees = 55f, followDamping = 8f;
        [SerializeField] private bool reduceShake = false;
        [SerializeField] private float zoom = 1f;   // < 1 brings the camera closer (the duel demo uses about 0.7)
        private Vector3 _shakeOffset; private float _shakeDecay;
        public float Zoom => zoom; public void SetZoom(float z)=>zoom=Mathf.Clamp(z,0.4f,1.6f); public void SetTarget(Transform t)=>target=t; public void SetReduceShake(bool v)=>reduceShake=v;
        private void LateUpdate()
        {
            if(target==null)return;
            Vector3 back=new Vector3(0f,height*zoom,-distance*zoom); Vector3 desired=target.position+back+_shakeOffset;
            transform.position=Vector3.Lerp(transform.position,desired,1f-Mathf.Exp(-followDamping*Time.deltaTime));
            transform.rotation=Quaternion.Euler(pitchDegrees,0f,0f);
            if(_shakeDecay>0f){ _shakeDecay-=Time.deltaTime; _shakeOffset=reduceShake?Vector3.zero:Random.insideUnitSphere*(0.25f*Mathf.Max(0f,_shakeDecay)); if(_shakeDecay<=0f) _shakeOffset=Vector3.zero; }
        }
        public void Shake(float s=0.25f){ if(reduceShake)return; _shakeDecay=Mathf.Max(_shakeDecay,s); }
    }
}
