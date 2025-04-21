using UnityEngine;

namespace Calculator
{
    public class CalculatorGraphProcessor : MonoBehaviour
    {
        [SerializeField] private Logger m_Logger;
        [SerializeField] private CalculatorGraph m_Graph;
        [SerializeField] private CalculatorNode[] m_Nodes;
    
        void Start()
        {
            foreach (var node in m_Nodes)
            {
                node.Logger = m_Logger;
            }

            m_Graph.Init();
            m_Graph.Process();
        }
    }
}