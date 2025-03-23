using GiftHorse.ScriptableGraphs.Attributes;

namespace Calculator
{
    [NodeScript("Calculator")]
    public class ValueNode : CalculatorNode
    {
        [Output] public float Value;

        [NodeField] public Logger Logger;
        [NodeField] public float Input;

        protected override void OnProcess()
        {
            Value = Input;
            Logger.AddLog($"Node: {Id}, has Depth Level: {DepthLevel} and Outputs value: {Value}");
        }
    }
}