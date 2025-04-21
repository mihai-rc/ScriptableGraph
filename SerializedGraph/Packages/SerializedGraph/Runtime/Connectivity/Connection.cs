using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs
{
    /// <summary>
    /// Data about a connection between two ports. 
    /// </summary>
    [Serializable]
    public class Connection
    {
        private const string k_FieldsCouldNotBeFound = "[ScriptableGraph] The field {0} of the associated port cannot be found!";
        
        [SerializeField] private string m_Id;

        [SerializeReference] private OutPort m_FromPort;
        [SerializeReference] private InPort m_ToPort;

        private Action m_TransferValueFn;

        /// <summary>
        /// The id of this <see cref="Connection"/>.
        /// </summary>
        public string Id => m_Id;

        /// <summary>
        /// The <see cref="OutPort"/> this <see cref="Connection"/> is taking its value from.
        /// </summary>
        public OutPort FromPort => m_FromPort;

        /// <summary>
        /// The <see cref="InPort"/> this <see cref="Connection"/> is giving its value to.
        /// </summary>
        public InPort ToPort => m_ToPort;

        /// <summary>
        /// <see cref="Connection"/> constructor.
        /// </summary>
        /// <param name="fromPort"> The <see cref="OutPort"/> this <see cref="Connection"/> is taking its value from. </param>
        /// <param name="toPort"> The <see cref="InPort"/> this <see cref="Connection"/> is giving its value to. </param>
        public Connection(OutPort fromPort, InPort toPort)
        {
            m_Id = Guid.NewGuid().ToString();
            m_FromPort = fromPort;
            m_ToPort = toPort;
        }

        /// <summary>
        /// Initializes the <see cref="Connection"/>.
        /// </summary>
        /// <param name="nodesById"> Dictionary of the <see cref="ScriptableNode"/>s mapped to their ids. </param>
        public void Init(Dictionary<string, ScriptableNode> nodesById) => m_TransferValueFn = CreateTransferValueDelegate(nodesById);
        
        /// <summary>
        /// Transfers the value from the emitting output field into the receiving input field.
        /// </summary>
        public void TransferValue() => m_TransferValueFn?.Invoke();

        /// <summary>
        /// Uses a simple expression tree to create a delegate that captures the fields
        /// corresponding to the connected ports and assigns the output to the input.
        /// </summary>
        /// <param name="nodesById">
        /// Dictionary of the <see cref="ScriptableNode"/>s mapped to their ids.
        /// </param>
        /// <returns>
        /// A delegate that assigns the input field of the receiving
        /// <see cref="ScriptableNode"/> to the output field of the emitting <see cref="ScriptableNode"/>.
        /// </returns>
        /// <remarks>
        /// In order for this to work, the fields of the nodes must be public and the <see cref="ScriptableGraph.Nodes"/>
        /// should be kept sorted by their Depth Level thus the output field is always assigned before the input field.
        /// TODO: Make sure that this approach works well with Apple policies. If not, change the solution to use code generation to achieve the same result.
        /// </remarks>
        private Action CreateTransferValueDelegate(Dictionary<string, ScriptableNode> nodesById)
        {
            var fromNode = nodesById[m_FromPort.NodeId];
            var toNode = nodesById[m_ToPort.NodeId];
            
            // Get field info by reflection
            var sourceField = fromNode.GetType().GetField(m_FromPort.Name);
            var targetField = toNode.GetType().GetField(m_ToPort.Name);

            if (sourceField == null)
            {
                Debug.LogErrorFormat(k_FieldsCouldNotBeFound, m_FromPort.Name);
                return null;
            }

            if (targetField == null)
            {
                Debug.LogErrorFormat(k_FieldsCouldNotBeFound, m_ToPort.Name);
                return null;
            }
            
            // Create expressions for field access
            var sourceExpression = Expression.Field(Expression.Constant(fromNode), sourceField);
            var targetExpression = Expression.Field(Expression.Constant(toNode), targetField);

            // Create assignment expression
            var assignExpression = Expression.Assign(targetExpression, sourceExpression);
            
            // Compile the expression into an Action
            return Expression.Lambda<Action>(assignExpression).Compile();
        }
    }
}