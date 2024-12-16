using UnityEngine;
using UnityEngine.UI;

public class SnowAnimation : MonoBehaviour
{
    public Image image;  // 연결된 SpriteRenderer
    public string folderName = "Snow";    // 이미지가 있는 폴더 이름
    public float frameDuration = 0.1f;    // 프레임당 지속 시간 (0.1초)

    private Sprite[] snowSprites;         // 스프라이트 배열
    private int currentFrame = 0;         // 현재 프레임 인덱스
    private float timer = 0f;             // 시간 측정을 위한 타이머

    void Start()
    {
        // Resources 폴더에서 스프라이트 로드
        snowSprites = Resources.LoadAll<Sprite>(folderName);

        if (snowSprites.Length == 0)
        {
            Debug.LogError($"No sprites found in Resources/{folderName}. Make sure the folder and files are correct.");
        }
    }

    void Update()
    {
        if (snowSprites == null || snowSprites.Length == 0) return;

        // 타이머 계산
        timer += Time.deltaTime;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;

            // 현재 프레임 업데이트 및 순환
            currentFrame = (currentFrame + 1) % snowSprites.Length;
            image.sprite = snowSprites[currentFrame];
        }
    }
}
