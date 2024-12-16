using UnityEngine;
using TMPro;

public class LoadingTextSelector : MonoBehaviour
{
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private string[] textPool = new string[] {};

    private void Start()
    {
        if (loadingText != null)
        {
            int randomIndex = GetUniqueRandomIndex();
            loadingText.text = textPool[randomIndex];
        }
    }

    private int GetUniqueRandomIndex()
    {
        if (textPool.Length <= 1) return 0;
        
        int[] indices = new int[textPool.Length];
        for (int i = 0; i < textPool.Length; i++)
            indices[i] = i;
            
        // Fisher-Yates
        for (int i = indices.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = indices[i];
            indices[i] = indices[randomIndex];
            indices[randomIndex] = temp;
        }
        
        return indices[0];
    }
}
