﻿using UnityEngine;
using System;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using GraphDataStore = UnityEditor.ShaderGraph.DataStore<UnityEditor.ShaderGraph.GraphData>;

namespace UnityEditor.ShaderGraph.Drawing
{
    class ChangeExposedFlagAction : IGraphDataAction
    {
        void ChangeExposedFlag(GraphData graphData)
        {
            Assert.IsNotNull(graphData, "GraphData is null while carrying out ChangeExposedFlagAction");
            Assert.IsNotNull(ShaderInputReference, "ShaderInputReference is null while carrying out ChangeExposedFlagAction");
            // The Undos are currently handled in ShaderInputPropertyDrawer but we want to move that out from there and handle here
            //graphData.owner.RegisterCompleteObjectUndo("Change Exposed Toggle");
            ShaderInputReference.generatePropertyBlock = NewIsExposedValue;
        }

        public Action<GraphData> ModifyGraphDataAction => ChangeExposedFlag;

        // Reference to the shader input being modified
        internal ShaderInput ShaderInputReference { get; set; }

        // New value of whether the shader input should be exposed to the material inspector

        internal bool NewIsExposedValue { get; set; }
    }

    class ChangePropertyValueAction : IGraphDataAction
    {
        void ChangePropertyValue(GraphData graphData)
        {
            Assert.IsNotNull(graphData, "GraphData is null while carrying out ChangePropertyValueAction");
            Assert.IsNotNull(ShaderPropertyReference, "ShaderInputReference is null while carrying out ChangePropertyValueAction");
            // The Undos are currently handled in ShaderInputPropertyDrawer but we want to move that out from there and handle here
            //graphData.owner.RegisterCompleteObjectUndo("Change Property Value");
            switch (ShaderPropertyReference)
            {
                case BooleanShaderProperty booleanProperty:
                    booleanProperty.value = ((ToggleData)NewShaderPropertyValue).isOn;
                    break;
                case Vector1ShaderProperty vector1Property:
                    vector1Property.value = (float)NewShaderPropertyValue;
                    break;
                case Vector2ShaderProperty vector2Property:
                    vector2Property.value = (Vector2)NewShaderPropertyValue;
                    break;
                case Vector3ShaderProperty vector3Property:
                    vector3Property.value = (Vector3)NewShaderPropertyValue;
                    break;
                case Vector4ShaderProperty vector4Property:
                    vector4Property.value = (Vector4)NewShaderPropertyValue;
                    break;
                case ColorShaderProperty colorProperty:
                    colorProperty.value = (Color)NewShaderPropertyValue;
                    break;
                case Texture2DShaderProperty texture2DProperty:
                    texture2DProperty.value.texture = (Texture)NewShaderPropertyValue;
                    break;
                case Texture2DArrayShaderProperty texture2DArrayProperty:
                    texture2DArrayProperty.value.textureArray = (Texture2DArray)NewShaderPropertyValue;
                    break;
                case Texture3DShaderProperty texture3DProperty:
                    texture3DProperty.value.texture = (Texture3D)NewShaderPropertyValue;
                    break;
                case CubemapShaderProperty cubemapProperty:
                    cubemapProperty.value.cubemap = (Cubemap)NewShaderPropertyValue;
                    break;
                case Matrix2ShaderProperty matrix2Property:
                    matrix2Property.value = (Matrix4x4)NewShaderPropertyValue;
                    break;
                case Matrix3ShaderProperty matrix3Property:
                    matrix3Property.value = (Matrix4x4)NewShaderPropertyValue;
                    break;
                case Matrix4ShaderProperty matrix4Property:
                    matrix4Property.value = (Matrix4x4)NewShaderPropertyValue;
                    break;
                case SamplerStateShaderProperty samplerStateProperty:
                    samplerStateProperty.value = (TextureSamplerState)NewShaderPropertyValue;
                    break;
                case GradientShaderProperty gradientProperty:
                    gradientProperty.value = (Gradient)NewShaderPropertyValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Action<GraphData> ModifyGraphDataAction => ChangePropertyValue;

        // Reference to the shader property being modified
        internal AbstractShaderProperty ShaderPropertyReference { get; set; }

        // New value of the shader property

        internal object NewShaderPropertyValue { get; set; }
    }

    class ChangeDisplayNameAction : IGraphDataAction
    {
        void ChangeDisplayName(GraphData graphData)
        {
            Assert.IsNotNull(graphData, "GraphData is null while carrying out ChangeDisplayNameAction");
            Assert.IsNotNull(ShaderInputReference, "ShaderInputReference is null while carrying out ChangeDisplayNameAction");
            // The Undos are currently handled in ShaderInputPropertyDrawer but we want to move that out from there and handle here
            //graphData.owner.RegisterCompleteObjectUndo("Change Display Name");
            if (NewDisplayNameValue != ShaderInputReference.displayName)
            {
                graphData.SanitizeGraphInputName(ShaderInputReference, NewDisplayNameValue);
            }
        }

        public Action<GraphData> ModifyGraphDataAction =>  ChangeDisplayName;

        // Reference to the shader input being modified
        internal ShaderInput ShaderInputReference { get; set; }

        internal string NewDisplayNameValue { get; set; }
    }

    class ChangeReferenceNameAction : IGraphDataAction
    {
        void ChangeReferenceName(GraphData graphData)
        {
            Assert.IsNotNull(graphData, "GraphData is null while carrying out ChangeReferenceNameAction");
            Assert.IsNotNull(ShaderInputReference, "ShaderInputReference is null while carrying out ChangeReferenceNameAction");
            // The Undos are currently handled in ShaderInputPropertyDrawer but we want to move that out from there and handle here
            //graphData.owner.RegisterCompleteObjectUndo("Change Reference Name");
            if (NewReferenceNameValue != ShaderInputReference.overrideReferenceName)
            {
                graphData.SanitizeGraphInputReferenceName(ShaderInputReference, NewReferenceNameValue);
            }
        }

        public Action<GraphData> ModifyGraphDataAction =>  ChangeReferenceName;

        // Reference to the shader input being modified
        internal ShaderInput ShaderInputReference { get; set; }

        internal string NewReferenceNameValue { get; set; }
    }

    class ResetReferenceNameAction : IGraphDataAction
    {
        void ResetReferenceName(GraphData graphData)
        {
            Assert.IsNotNull(graphData, "GraphData is null while carrying out ResetReferenceNameAction");
            Assert.IsNotNull(ShaderInputReference, "ShaderInputReference is null while carrying out ResetReferenceNameAction");
            graphData.owner.RegisterCompleteObjectUndo("Reset Reference Name");
            ShaderInputReference.overrideReferenceName = null;
        }

        public Action<GraphData> ModifyGraphDataAction =>  ResetReferenceName;

        // Reference to the shader input being modified
        internal ShaderInput ShaderInputReference { get; set; }
    }

    // TODO: GraphView handles deletion of selected items using MaterialGraphView::DeleteSelectionImplementation(), which keeps all child views out of the loop
    // And forces state tracking hacks like the tracking of removed inputs etc, currently use a delegate called at input removal to handle BB cleanup
    // Find a better way that uses GraphDataActions instead
    class DeleteShaderInputAction : IGraphDataAction
    {
        void DeleteShaderInput(GraphData graphData)
        {
            Assert.IsNotNull(graphData, "GraphData is null while carrying out DeleteShaderInputAction");
            Assert.IsNotNull(ShaderInputReference, "ShaderInputReference is null while carrying out DeleteShaderInputAction");
            graphData.owner.RegisterCompleteObjectUndo("Delete Graph Input");
            graphData.RemoveGraphInput(ShaderInputReference);
        }

        public Action<GraphData> ModifyGraphDataAction =>  DeleteShaderInput;

        // Reference to the shader input being deleted
        internal ShaderInput ShaderInputReference { get; set; }

    }

    class ShaderInputViewController : SGViewController<ShaderInput, ShaderInputViewModel>
    {
        // Exposed for PropertyView
        internal GraphData DataStoreState => DataStore.State;

        internal ShaderInputViewController(ShaderInput shaderInput, ShaderInputViewModel inViewModel, GraphDataStore graphDataStore)
            : base(shaderInput, inViewModel, graphDataStore)
        {
            InitializeViewModel();

            m_BlackboardPropertyView = new BlackboardPropertyView(ViewModel);

            m_BlackboardPropertyView.controller = this;

            m_BlackboardRowView = new SGBlackboardRow(m_BlackboardPropertyView, null);
            m_BlackboardRowView.expanded = SessionState.GetBool($"Unity.ShaderGraph.Input.{shaderInput.objectId}.isExpanded", false);
        }

        void InitializeViewModel()
        {
            ViewModel.Model = Model;
            ViewModel.IsSubGraph = DataStore.State.isSubGraph;
            ViewModel.IsInputExposed = (DataStore.State.isSubGraph || (Model.isExposable && Model.generatePropertyBlock));
            ViewModel.InputName = Model.displayName;
            switch (Model)
            {
                case AbstractShaderProperty shaderProperty:
                    ViewModel.InputTypeName = shaderProperty.GetPropertyTypeString();
                    // HACK: Handles upgrade fix for deprecated old Color property
                    shaderProperty.onBeforeVersionChange += (_) => DataStoreState.owner.RegisterCompleteObjectUndo($"Change {shaderProperty.displayName} Version");
                    break;
                case ShaderKeyword shaderKeyword:
                    ViewModel.InputTypeName = shaderKeyword.keywordType  + " Keyword";
                    ViewModel.InputTypeName = shaderKeyword.isBuiltIn ? "Built-in " + ViewModel.InputTypeName : ViewModel.InputTypeName;
                    break;
            }

            ViewModel.RequestModelChangeAction = this.RequestModelChange;
        }

        SGBlackboardRow m_BlackboardRowView;
        BlackboardPropertyView m_BlackboardPropertyView;

        internal SGBlackboardRow BlackboardItemView => m_BlackboardRowView;

        protected override void RequestModelChange(IGraphDataAction changeAction)
        {
            DataStore.Dispatch(changeAction);
        }

        // Called by GraphDataStore.Subscribe after the model has been changed
        protected override void ModelChanged(GraphData graphData, IGraphDataAction changeAction)
        {
            if (changeAction is ChangeExposedFlagAction changeExposedFlagAction)
            {
                ViewModel.IsInputExposed = Model.generatePropertyBlock;
                DirtyNodes(ModificationScope.Graph);
                m_BlackboardPropertyView.UpdateFromViewModel();
            }
            else if (changeAction is ChangePropertyValueAction changePropertyValueAction)
            {
                DirtyNodes(ModificationScope.Graph);
                m_BlackboardPropertyView.MarkDirtyRepaint();
            }
            else if (changeAction is ResetReferenceNameAction resetReferenceNameAction)
            {
                DirtyNodes(ModificationScope.Graph);
            }
            else if (changeAction is ChangeReferenceNameAction changeReferenceNameAction)
            {
                // TODO: Handle reset reference name menu behavior here??
                DirtyNodes(ModificationScope.Graph);
            }
            else if (changeAction is ChangeDisplayNameAction changeDisplayNameAction)
            {
                ViewModel.InputName = Model.displayName;
                DirtyNodes(ModificationScope.Topological);
                m_BlackboardPropertyView.UpdateFromViewModel();
            }
        }

        // TODO: This should communicate to node controllers instead of searching for the nodes themselves everytime, but that's going to take a while...
        internal void DirtyNodes(ModificationScope modificationScope = ModificationScope.Node)
        {
            switch (Model)
            {
                case AbstractShaderProperty property:
                    var graphEditorView = m_BlackboardRowView.GetFirstAncestorOfType<GraphEditorView>();
                    if (graphEditorView == null)
                        return;
                    var colorManager = graphEditorView.colorManager;
                    var nodes = graphEditorView.graphView.Query<MaterialNodeView>().ToList();

                    colorManager.SetNodesDirty(nodes);
                    colorManager.UpdateNodeViews(nodes);

                    foreach (var node in DataStore.State.GetNodes<PropertyNode>())
                    {
                        node.Dirty(modificationScope);
                    }
                    break;
                case ShaderKeyword keyword:
                    foreach (var node in DataStore.State.GetNodes<KeywordNode>())
                    {
                        node.UpdateNode();
                        node.Dirty(modificationScope);
                    }

                    // Cant determine if Sub Graphs contain the keyword so just update them
                    foreach (var node in DataStore.State.GetNodes<SubGraphNode>())
                    {
                        node.Dirty(modificationScope);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
