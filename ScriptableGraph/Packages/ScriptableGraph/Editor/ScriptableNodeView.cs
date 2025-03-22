using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace GiftHorse.ScriptableGraphs.Editor
{
    public class ScriptableNodeView : Node
    {
        private const string k_PropertiesHolderName = "PropertiesHolder";
        private const string k_PropertyFieldName = "PropertyField";
        private const string k_IdProperty = "m_Id";
        private const string k_NodesProperty = "m_Nodes";
        private const string k_SerializedPropertyNotFoundError = "[Editor] [ScriptableGraph] Could not find the SerializedProperty of ScriptableGraphNode: {0}, Id: {1}.";
        private const string k_InvalidNodeProperty = "[Editor] [ScriptableGraph] Could not find the relative property by name: {0}. Make sure you use [NodeField] attribute only with serialized types.";
        
        private readonly ScriptableNode m_ScriptableNode;
        private readonly ScriptableGraphEditorContext m_Context;
        private SerializedProperty m_SerializedProperty;
        private VisualElement m_PropertiesHolder;
        
        public ScriptableNode ScriptableNode => m_ScriptableNode;

        public List<Port> InPorts { get; } = new();
        public List<Port> OutPorts { get; } = new();

        public ScriptableNodeView(ScriptableNode scriptableNode, ScriptableGraphEditorContext context)
        {
            m_ScriptableNode = scriptableNode;
            m_Context = context;
            
            CreateInputs();
            CreateOutputs();
            InitializeNodeByReflection();
        }
        
        public void SavePosition()
        {
            ScriptableNode.Position = GetPosition();
        }

        protected override void ToggleCollapse()
        {
            base.ToggleCollapse();
            
            m_ScriptableNode.Expanded = expanded;
            m_Context.MarkAssetAsDirty();
        }

        private VisualElement InitializePropertiesHolder()
        {
            var propertiesHolder = new VisualElement();
            propertiesHolder.name = k_PropertiesHolderName;
            
            return propertiesHolder;
        }
        
        private SerializedProperty InitializeSerializedProperty()
        {
            m_Context.SerializedObject.Update();
            
            var nodes = m_Context.SerializedObject.FindProperty(k_NodesProperty);
            if (!nodes.isArray)
            {
                Debug.LogErrorFormat(k_SerializedPropertyNotFoundError, name, m_ScriptableNode.Id);
                return null;
            }
            
            var size = nodes.arraySize;
            for (var i = 0; i < size; i++)
            {
                var element = nodes.GetArrayElementAtIndex(i);
                var elementId = element.FindPropertyRelative(k_IdProperty);
                    
                if (!elementId.stringValue.Equals(ScriptableNode.Id)) 
                    continue;
                    
                return element;
            }

            Debug.LogErrorFormat(k_SerializedPropertyNotFoundError, name, m_ScriptableNode.Id);
            return null;
        }

        private void SetupNodeHeaderByReflection(Type type)
        {
            name = ScriptableNode.Title;
            title = ScriptableNode.Title;
            
            if (!ReflectionHelper.TryGetNodeHeaderColor(type, out var color)) 
                return;
            
            titleContainer.style.backgroundColor = new StyleColor(color);
            titleContainer.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        private void InitializeNodeByReflection()
        {
            var type = ScriptableNode.GetType();
            SetupNodeHeaderByReflection(type);
            GetNodePropertiesByReflection(type);
        }

        private void CreateInputs()
        {
            foreach (var inPort in m_ScriptableNode.InPorts)
            {
                CreateInputPort(inPort.Name, Type.GetType(inPort.CompatibleType), false);
            }
        }
        
        private void CreateOutputs()
        {
            foreach (var outPort in m_ScriptableNode.OutPorts)
            {
                CreateOutputPort(outPort.Name, Type.GetType(outPort.CompatibleType), true);
            }
        }

        private void GetNodePropertiesByReflection(Type type)
        {
            foreach (var propertyName in ReflectionHelper.GetNodeExposedFieldsNames(type))
            {
                if (m_PropertiesHolder is null)
                {
                    m_PropertiesHolder = InitializePropertiesHolder();
                }
                
                DrawProperty(propertyName);
            }
            
            extensionContainer.Add(m_PropertiesHolder);
            RefreshExpandedState();
        }

        private void CreateInputPort(string portName, Type portType, bool multiple = false)
        {
            var capacity = multiple ? Port.Capacity.Multi : Port.Capacity.Single;
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, portType);
            inputPort.portName = portName;
            
            InPorts.Add(inputPort);
            inputContainer.Add(inputPort);
        }

        private void CreateOutputPort(string portName, Type portType, bool multiple = false)
        {
            var capacity = multiple ? Port.Capacity.Multi : Port.Capacity.Single;
            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, portType);
            outputPort.portName = portName;
            
            OutPorts.Add(outputPort);
            outputContainer.Add(outputPort);
        }
        
        private PropertyField DrawProperty(string propertyName)
        {
            if (m_SerializedProperty is null)
            {
                m_SerializedProperty = InitializeSerializedProperty();
            }

            var property = m_SerializedProperty.FindPropertyRelative(propertyName);
            if (property is null)
            {
                Debug.LogErrorFormat(k_InvalidNodeProperty, propertyName);
                return null;
            }

            var propertyField = new PropertyField(property);
            propertyField.name = k_PropertyFieldName;
            propertyField.Bind(property.serializedObject);
            
            m_PropertiesHolder.Add(propertyField);
            
            return propertyField;
        }
    }
}
