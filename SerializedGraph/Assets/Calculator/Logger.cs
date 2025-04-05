using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Calculator
{
    public class Logger : MonoBehaviour
    {
        [SerializeField] private Text m_ConsoleText;
        private readonly StringBuilder m_LogBuilder = new();

        public void AddLog(string log)
        {
            m_LogBuilder.AppendLine(log);
        }

        public void ShowLog()
        {
            if (m_ConsoleText != null)
                m_ConsoleText.text = m_LogBuilder.ToString();
        
            m_LogBuilder.Clear();
        }
    }
}
