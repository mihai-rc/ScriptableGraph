using GiftHorse.SerializedGraphs;
using UnityEngine;

namespace Calculator
{
    [CreateAssetMenu(fileName = "CalculatorGraph", menuName = "CalculatorGraph")]
    public class CalculatorGraph : SerializedGraphOf<CalculatorNode>
    {
    }
}
