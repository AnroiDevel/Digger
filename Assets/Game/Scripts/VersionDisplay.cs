using UnityEngine;
using UnityEngine.UI;

namespace MultiTool
{
    [ExecuteInEditMode]
    public class VersionDisplay : MonoBehaviour
    {
        private const string VersionKey = "LastLaunchVersion";
        [SerializeField] private Text _versionText; // UI ������� ��� ����������� ������

        private void Awake()
        {
#if UNITY_EDITOR
            UpdateVersion();
            DisplayVersion();
#endif
        }

#if UNITY_EDITOR
        private void UpdateVersion()
        {
            // �������� ������� ���� � ������� ������
            string currentVersion = System.DateTime.Now.ToString("ddMMyy");

            // ��������� ������ � PlayerPrefs
            PlayerPrefs.SetString(VersionKey, currentVersion);
            PlayerPrefs.Save();
        }

        private void DisplayVersion()
        {
            // ���������, ���������� �� UI �������
            if(_versionText != null)
            {
                // �������� ����������� ������
                string lastLaunchVersion = PlayerPrefs.GetString(VersionKey, "000000");
                _versionText.text = "Version: " + lastLaunchVersion;
            }
            else
            {
                Debug.LogError("Version Text is not assigned.");
            }
        }
#endif
    }
}
