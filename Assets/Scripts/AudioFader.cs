using UnityEngine;
using System.Collections;

public class AudioFader : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Settings")]
    public float delayBeforeFade = 3f; // จะให้เสียงดังค้างไว้นานแค่ไหนก่อนเริ่ม Fade
    public float fadeDuration = 3f;    // ใช้เวลา Fade นานแค่ไหน (ยิ่งเยอะยิ่งนุ่ม)

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // เริ่มทำงาน Coroutine สำหรับ Fade ออก
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        // 1. รอตามเวลาที่กำหนดก่อนเริ่ม Fade
        yield return new WaitForSeconds(delayBeforeFade);

        float startVolume = audioSource.volume;

        // 2. ค่อยๆ ลด Volume ลงตามเวลา
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            // คำนวณค่า Volume ใหม่ (ค่อยๆ ลดจากค่าเริ่มต้นไป 0)
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        // 3. มั่นใจว่าเสียงเงียบสนิทแล้วสั่งหยุด
        audioSource.volume = 0;
        audioSource.Stop();
    }
}