using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake
{
    public static IEnumerator ScreenShakeEffect(float duration, float magnitude)
    {
        Vector3 ogPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(ogPos.x + x, ogPos.y + y, ogPos.z);

            elapsed += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        Camera.main.transform.localPosition = ogPos;
    }

   
}
