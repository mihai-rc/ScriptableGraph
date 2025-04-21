using GiftHorse.ScriptableGraphs.Attributes;

namespace Calculator
{
    [NodeScript]
    public class PrintNode : CalculatorNode
    {
        [Input] public float Value;

        protected override void OnProcess()
        {
            Logger.AddLog($"Node: {Id}, has Depth Level: {DepthLevel}. PRINT: {Value}");
            Logger.ShowLog();
        }
    }
}