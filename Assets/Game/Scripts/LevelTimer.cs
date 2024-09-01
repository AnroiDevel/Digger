using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private Text _timerText; // ������ �� ��������� ��������� UI
    private float _elapsedTime;              // ��������� ����� � ��������
    private bool _isRunning;                 // ���� ��������� �������


    private void Update()
    {
        if(_isRunning)
        {
            _elapsedTime += Time.deltaTime; // ����������� ����� �� ��������� ����� � ����������� �����
            UpdateTimerText(); // ��������� ����� ����������� �������
        }
    }

    public void StartTimer()
    {
        _isRunning = true;  // ������������� ���� ������ �������
        _elapsedTime = 0f;  // ���������� ��������� �����
        gameObject.SetActive(true);
    }

    public void StopTimer()
    {
        _isRunning = false; // ������������� ������
        gameObject.SetActive(false);
    }

    private void UpdateTimerText()
    {
        int minutes = (int)(_elapsedTime / 60);   // ��������� ������
        int seconds = (int)(_elapsedTime % 60);   // ��������� �������

        _timerText.text = $"{minutes:D2}:{seconds:D2}"; // ��������� ��������� ���������
    }

    internal int GetTime()
    {
        return (int)_elapsedTime;
    }
}
