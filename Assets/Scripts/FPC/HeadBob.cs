using System;
using UnityEngine;
using UnityStandardAssets.Utility;
using System.Collections;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class HeadBob : MonoBehaviour
    {
        public Camera Camera;
        public CurveControlledBob motionBob = new CurveControlledBob();
        public LerpControlledBob jumpAndLandingBob = new LerpControlledBob();
        public RigidbodyFirstPersonController rigidbodyFirstPersonController;
        public float StrideInterval;
        [Range(0f, 1f)]
        public float RunningStrideLengthen;

        // private CameraRefocus m_CameraRefocus;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;

        //Screen shake stuff
        public float duration = 0.5f;
        public float speed = 1.0f;
        public float magnitude = 0.1f;

        float camShakeX = 0;
        float camShakeY = 0;

        private void Start()
        {
            motionBob.Setup(Camera, StrideInterval);
            m_OriginalCameraPosition = Camera.transform.localPosition;
            PlayShake();
            //     m_CameraRefocus = new CameraRefocus(Camera, transform.root.transform, Camera.transform.localPosition);
        }


        private void Update()
        {
            //  m_CameraRefocus.GetFocusPoint();
            Vector3 newCameraPosition;
            if (rigidbodyFirstPersonController.Velocity.magnitude > 0 && rigidbodyFirstPersonController.Grounded)
            {
                //Headbob
                Camera.transform.localPosition = motionBob.DoHeadBob(rigidbodyFirstPersonController.Velocity.magnitude * (rigidbodyFirstPersonController.Running ? RunningStrideLengthen : 1f));

                //Shakey shakey
                Vector3 cp = Camera.transform.localPosition;
                Camera.transform.localPosition = new Vector3(cp.x + camShakeX, cp.y + camShakeY,cp.z);

                //Update
                newCameraPosition = Camera.transform.localPosition;
                newCameraPosition.y = Camera.transform.localPosition.y - jumpAndLandingBob.Offset();
            }
            else
            {
                newCameraPosition = Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - jumpAndLandingBob.Offset();
            }
            Camera.transform.localPosition = newCameraPosition;

            if (!m_PreviouslyGrounded && rigidbodyFirstPersonController.Grounded)
            {
                StartCoroutine(jumpAndLandingBob.DoBobCycle());
            }

            m_PreviouslyGrounded = rigidbodyFirstPersonController.Grounded;
            //  m_CameraRefocus.SetFocusPoint();
        }

        //From http://unitytipsandtricks.blogspot.com.au/2013/05/camera-shake.html
        public void PlayShake()
        {
            StopAllCoroutines();
            StartCoroutine("Shake");
        }

        IEnumerator Shake()
        {
            float elapsed = 0.0f;

            float randomStart = UnityEngine.Random.Range(-1000.0f, 1000.0f);

            while (elapsed < duration)
            {

                elapsed += Time.deltaTime;

                float percentComplete = elapsed / duration;

                // We want to reduce the shake from full power to 0 starting half way through
                float damper = 1.0f - Mathf.Clamp(2.0f * percentComplete - 1.0f, 0.0f, 1.0f);

                // Calculate the noise parameter starting randomly and going as fast as speed allows
                float alpha = randomStart + speed * percentComplete;

                // map noise to [-1, 1]
                //float x = Util.Noise.GetNoise(alpha, 0.0f, 0.0f) * 2.0f - 1.0f;
                //float y = Util.Noise.GetNoise(0.0f, alpha, 0.0f) * 2.0f - 1.0f;

                float x = Mathf.PerlinNoise(alpha, 0) * 2.0f - 1.0f;
                float y = Mathf.PerlinNoise(0, alpha) * 2.0f - 1.0f;

                x *= magnitude * damper;
                y *= magnitude * damper;

                //Camera.main.transform.position = new Vector3(x, y, originalCamPos.z);
                camShakeX = x;
                camShakeY = y;

                yield return null;
            }

            //Camera.main.transform.position = originalCamPos;
            camShakeX = 0;
            camShakeY = 0;
        }
    }
}
