using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UnfrozenTestWork
{
    public class SceneFader : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve _curve;

        public IEnumerator FadeIn(Image image)
        {
            float t = 1f;

            while (t > 0f)
            {
                t -= Time.deltaTime;
                float a = _curve.Evaluate(t);
                Color color = image.color;
                color.a = a;
                image.color = color;
                yield return null;
            }
        }

        public IEnumerator FadeOut(Image image)
        {
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime;
                float a = _curve.Evaluate(t);
                Color color = image.color;
                color.a = a;
                image.color = color;
                yield return null;
            }
        }
    }
}
