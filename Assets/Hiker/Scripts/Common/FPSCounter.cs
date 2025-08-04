using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text Text;

    private Dictionary<int, string> CachedNumberStrings = new Dictionary<int, string>();
    private int[] _frameRateSamples;
    private int _cacheNumbersAmount = 300;
    private int _averageFromAmount = 30;
    private int _averageCounter = 0;
    private int _currentAveraged;

    void Awake()
    {
        // Cache strings and create array
        {
            for (int i = 0; i < _cacheNumbersAmount; i++)
            {
                CachedNumberStrings[i] = string.Format("{0} FPS", i);
            }
            _frameRateSamples = new int[_averageFromAmount];
        }
    }
    void Update()
    {
        // Sample
        {
            var currentFrame = (int)Mathf.Round(1f / Time.smoothDeltaTime); // If your game modifies Time.timeScale, use unscaledDeltaTime and smooth manually (or not).
            _frameRateSamples[_averageCounter] = currentFrame;
        }

        // Average
        {
            var average = 0f;

            foreach (var frameRate in _frameRateSamples)
            {
                average += frameRate;
            }

            _currentAveraged = (int)Mathf.Round(average / _averageFromAmount);
            _averageCounter = (_averageCounter + 1) % _averageFromAmount;
        }

        // Assign to UI
        {
            if (_currentAveraged >= 0 && _currentAveraged < _cacheNumbersAmount)
            {
                Text.text = CachedNumberStrings[_currentAveraged];
            }
            else if (_currentAveraged < 0)
            {
                Text.text = "< 0";
            }
            else if (_currentAveraged >= _cacheNumbersAmount)
            {
                Text.text = "> " + _cacheNumbersAmount;
            }
            else
            {
                Text.text = "?";
            }
        }
    }
}
