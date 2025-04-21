using GiftHorse.SerializedGraphs.Attributes;

namespace Calculator
{
    [NodeScript]
    public class PrintNode : CalculatorNode
    {
        [Input] public float Value;
        // [NodeField] public LoggerProperty LoggerProperty;

        protected override void OnProcess()
        {
            // LoggerProperty.Logger.AddLog($"Node: {Id}, has Depth Level: {DepthLevel}. PRINT: {Value}");
            // LoggerProperty.Logger.ShowLog();
        }
    }
}