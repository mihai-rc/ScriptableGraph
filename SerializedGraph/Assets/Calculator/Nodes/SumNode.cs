using GiftHorse.SerializedGraphs.Attributes;

namespace Calculator
{
    [NodeScript("Calculator"), HeaderColor(0.2f, 0.4f, 0.4f)]
    public class SumNode : CalculatorNode
    {
        [Input] public float First;
        [Input] public float Second;

        [Output] public float Value;
        // [NodeField] public LoggerProperty LoggerProperty;

        protected override void OnProcess()
        {
            Value = First + Second;
            // LoggerProperty.Logger.AddLog($"Node: {Id}, has Depth Level: {DepthLevel} and Outputs value: {Value}");
        }
    }
}