using GiftHorse.SerializedGraphs.Attributes;

namespace Calculator
{
    [NodeScript("Calculator")]
    public class ValueNode : CalculatorNode
    {
        [Output] public float Value;

        // [NodeField] public LoggerProperty LoggerProperty;
        [NodeField] public float Input;

        protected override void OnProcess()
        {
            Value = Input;
            // LoggerProperty.Logger.AddLog($"Node: {Id}, has Depth Level: {DepthLevel} and Outputs value: {Value}");
        }
    }
}