using UnityEngine;
using System;

public class Test : MonoBehaviour
{
    [SerializeField] private string input;
    private string result = null;

    private void OnValidate()
    {
        result = string.Empty;
        GetMinPalidrome();
    }

    public void GetMinPalidrome()
    {
        for(int len = 2; len <= 3; len++)
        {
            for(int i = 0; i <= input.Length - len; i++)
            {
                string subStr = input.Substring(i, len);

                if(IsPalindrome(subStr))
                {
                    if(result == string.Empty || IsLexicografityBest(subStr))
                    {
                        result = subStr;
                    }
                }
            }

        }

        result = result != string.Empty ? result : "-1";
        Debug.Log(result);
    }

    public bool IsLexicografityBest(string test)
    {
        if(test.Length > result.Length)
            return false;

        for(int i = 0; i < test.Length; i++)
        {
            if(test[i] < result[i])
                return true;
            else
                continue;
        }

        return false;
    }

    // Метод для проверки, является ли строка палиндромом
    public bool IsPalindrome(string str)
    {
        int left = 0;
        int right = str.Length - 1;

        while(left < right)
        {
            if(str[left] != str[right])
            {
                return false;
            }
            left++;
            right--;
        }
        return true;
    }
}
