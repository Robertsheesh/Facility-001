using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 2f;
    public float initialDelay = 5f; // Time to stay black before fading in

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        // Keep the screen black for the initial delay
        yield return new WaitForSeconds(initialDelay);

        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 1; // Start fully black
        fadeImage.color = color;

        // Gradually fade from black to transparent
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Ensure the final alpha is fully transparent
        color.a = 0f;
        fadeImage.color = color;

        // Optionally disable the image to stop rendering
        fadeImage.gameObject.SetActive(false);
    }
}
