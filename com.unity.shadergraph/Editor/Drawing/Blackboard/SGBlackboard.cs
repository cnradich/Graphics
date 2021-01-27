using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Drawing.Views;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.ShaderGraph;

namespace UnityEditor.ShaderGraph.Drawing
{
    class BlackboardGroupInfo
    {
        [SerializeField]
        SerializableGuid m_Guid = new SerializableGuid();

        internal Guid guid => m_Guid.guid;

        [SerializeField]
        string m_GroupName;

        internal string GroupName
        {
            get => m_GroupName;
            set => m_GroupName = value;
        }

        BlackboardGroupInfo()
        {

        }
    }

    class SGBlackboard : GraphSubWindow, ISGControlledElement<BlackboardController>
    {
        VisualElement m_ScrollBoundaryTop;
        VisualElement m_ScrollBoundaryBottom;
        VisualElement m_BottomResizer;
        TextField m_PathLabelTextField;

        // --- Begin ISGControlledElement implementation
        public void OnControllerChanged(ref SGControllerChangedEvent e)
        {

        }

        public void OnControllerEvent(SGControllerEvent e)
        {

        }

        public BlackboardController controller
        {
            get => m_Controller;
            set
            {
                if (m_Controller != value)
                {
                    if (m_Controller != null)
                    {
                        m_Controller.UnregisterHandler(this);
                    }
                    Clear();
                    m_Controller = value;

                    if (m_Controller != null)
                    {
                        m_Controller.RegisterHandler(this);
                    }
                }
            }
        }

        SGController ISGControlledElement.controller => m_Controller;

        // --- ISGControlledElement implementation

        BlackboardController m_Controller;

        BlackboardViewModel m_ViewModel;

        BlackboardViewModel ViewModel
        {
            get => m_ViewModel;
            set => m_ViewModel = value;
        }

        readonly SGBlackboardSection m_DefaultPropertySection;
        readonly SGBlackboardSection m_DefaultKeywordSection;

        // List of user-made blackboard sections
        IList<SGBlackboardSection> m_BlackboardSections = new List<SGBlackboardSection>();

        bool m_ScrollToTop = false;
        bool m_ScrollToBottom = false;
        bool m_EditPathCancelled = false;
        bool m_IsFieldBeingDragged = false;

        const int k_DraggedPropertyScrollSpeed = 6;

        public override string windowTitle => "Blackboard";
        public override string elementName => "SGBlackboard";
        public override string styleName => "Blackboard";
        public override string UxmlName => "GraphView/Blackboard";
        public override string layoutKey => "UnityEditor.ShaderGraph.Blackboard";

        public Action addItemRequested { get; set; }
        public Action<int, VisualElement> moveItemRequested { get; set; }

        GenericMenu m_AddPropertyMenu;

        public SGBlackboard(BlackboardViewModel viewModel) : base(viewModel)
        {
            ViewModel = viewModel;

            InitializeAddPropertyMenu();

            // By default dock blackboard to left of graph window
            windowDockingLayout.dockingLeft = true;

            if (m_MainContainer.Q(name: "addButton") is Button addButton)
                addButton.clickable.clicked += () =>
                {
                    addItemRequested?.Invoke();
                    ShowAddPropertyMenu();
                };

            ParentView.RegisterCallback<FocusOutEvent>(evt => HideScrollBoundaryRegions());

            // These make sure that the drag indicators are disabled whenever a drag action is cancelled without completing a drop
            this.RegisterCallback<MouseUpEvent>(evt =>
            {
                m_DefaultPropertySection.OnDragActionCanceled();
                m_DefaultKeywordSection.OnDragActionCanceled();
            });

            this.RegisterCallback<DragExitedEvent>(evt =>
            {
                m_DefaultPropertySection.OnDragActionCanceled();
                m_DefaultKeywordSection.OnDragActionCanceled();
            });

            m_SubTitleLabel.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);

            m_PathLabelTextField = new TextField { visible = false };
            m_PathLabelTextField.Q("unity-text-input").RegisterCallback<FocusOutEvent>(e => { OnEditPathTextFinished(); });
            m_PathLabelTextField.Q("unity-text-input").RegisterCallback<KeyDownEvent>(OnPathTextFieldKeyPressed);

            // These callbacks make sure the scroll boundary regions don't show up user is not dragging/dropping properties
            this.RegisterCallback<MouseUpEvent>((evt => HideScrollBoundaryRegions()));
            this.RegisterCallback<DragExitedEvent>(evt => HideScrollBoundaryRegions());

            m_ScrollBoundaryTop = m_MainContainer.Q(name: "scrollBoundaryTop");
            m_ScrollBoundaryTop.RegisterCallback<MouseEnterEvent>(ScrollRegionTopEnter);
            m_ScrollBoundaryTop.RegisterCallback<DragUpdatedEvent>(OnFieldDragUpdate);
            m_ScrollBoundaryTop.RegisterCallback<MouseLeaveEvent>(ScrollRegionTopLeave);

            m_ScrollBoundaryBottom = m_MainContainer.Q(name: "scrollBoundaryBottom");
            m_ScrollBoundaryBottom.RegisterCallback<MouseEnterEvent>(ScrollRegionBottomEnter);
            m_ScrollBoundaryBottom.RegisterCallback<DragUpdatedEvent>(OnFieldDragUpdate);
            m_ScrollBoundaryBottom.RegisterCallback<MouseLeaveEvent>(ScrollRegionBottomLeave);

            m_BottomResizer = m_MainContainer.Q("bottom-resize");

            HideScrollBoundaryRegions();

            // Sets delegate association so scroll boundary regions are hidden when a blackboard property is dropped into graph
            if (ParentView is MaterialGraphView materialGraphView)
                materialGraphView.blackboardFieldDropDelegate = HideScrollBoundaryRegions;

            isWindowScrollable = true;
            isWindowResizable = true;
            focusable = true;

            // Want to retain properties and keywords UI, but need to iterate through the GroupInfos, and create sections for each of those
            // Then for each section, add the corresponding properties and keywords based on their GUIDs
            m_DefaultPropertySection =  this.Q<SGBlackboardSection>("propertySection");
            m_DefaultKeywordSection = this.Q<SGBlackboardSection>("keywordSection");

            // TODO: Need to create a PropertyViewModel that is used to drive a BlackboardRow/FieldView
            // Also, given how similar the NodeView and FieldView are, would be awesome if we could just unify the two and get rid of FieldView
            // Then would theoretically also get the ability to connect properties from blackboard to node inputs directly
            // (could handle in controller to create a new PropertyNodeView and connect that instead)
        }

        public void ShowScrollBoundaryRegions()
        {
            if (!m_IsFieldBeingDragged && scrollableHeight > 0)
            {
                // Interferes with scrolling functionality of properties with the bottom scroll boundary
                m_BottomResizer.style.visibility = Visibility.Hidden;

                m_IsFieldBeingDragged = true;
                var contentElement = m_MainContainer.Q(name: "content");
                scrollViewIndex = contentElement.IndexOf(m_ScrollView);
                contentElement.Insert(scrollViewIndex, m_ScrollBoundaryTop);
                scrollViewIndex = contentElement.IndexOf(m_ScrollView);
                contentElement.Insert(scrollViewIndex + 1, m_ScrollBoundaryBottom);
            }
        }

        public void HideScrollBoundaryRegions()
        {
            m_BottomResizer.style.visibility = Visibility.Visible;
            m_IsFieldBeingDragged = false;
            m_ScrollBoundaryTop.RemoveFromHierarchy();
            m_ScrollBoundaryBottom.RemoveFromHierarchy();
        }

        int scrollViewIndex { get; set; }

        void ScrollRegionTopEnter(MouseEnterEvent mouseEnterEvent)
        {
            if (m_IsFieldBeingDragged)
            {
                m_ScrollToTop = true;
                m_ScrollToBottom = false;
            }
        }

        void ScrollRegionTopLeave(MouseLeaveEvent mouseLeaveEvent)
        {
            if (m_IsFieldBeingDragged)
                m_ScrollToTop = false;
        }

        void ScrollRegionBottomEnter(MouseEnterEvent mouseEnterEvent)
        {
            if (m_IsFieldBeingDragged)
            {
                m_ScrollToBottom = true;
                m_ScrollToTop = false;
            }
        }

        void ScrollRegionBottomLeave(MouseLeaveEvent mouseLeaveEvent)
        {
            if (m_IsFieldBeingDragged)
                m_ScrollToBottom = false;
        }

        void OnFieldDragUpdate(DragUpdatedEvent dragUpdatedEvent)
        {
            if (m_ScrollToTop)
                m_ScrollView.scrollOffset = new Vector2(m_ScrollView.scrollOffset.x, Mathf.Clamp(m_ScrollView.scrollOffset.y - k_DraggedPropertyScrollSpeed, 0, scrollableHeight));
            else if (m_ScrollToBottom)
                m_ScrollView.scrollOffset = new Vector2(m_ScrollView.scrollOffset.x, Mathf.Clamp(m_ScrollView.scrollOffset.y + k_DraggedPropertyScrollSpeed, 0, scrollableHeight));
        }

        public void HideAllDragIndicators()
        {

        }

        void InitializeAddPropertyMenu()
        {
            m_AddPropertyMenu = new GenericMenu();

            if (ViewModel == null)
            {
                Debug.Log("ERROR: SGBlackboard: View Model is null.");
                return;
            }

            foreach (var nameToAddActionTuple in ViewModel.PropertyNameToAddActionMap)
            {
                string propertyName = nameToAddActionTuple.Key;
                IGraphDataAction addAction = nameToAddActionTuple.Value;
                m_AddPropertyMenu.AddItem(new GUIContent(propertyName), false, ()=> addAction.ModifyGraphDataAction(ViewModel.Model));
            }
            m_AddPropertyMenu.AddSeparator($"/");

            foreach (var nameToAddActionTuple in ViewModel.DefaultKeywordNameToAddActionMap)
            {
                string defaultKeywordName = nameToAddActionTuple.Key;
                IGraphDataAction addAction = nameToAddActionTuple.Value;
                m_AddPropertyMenu.AddItem(new GUIContent($"Keyword/{defaultKeywordName}"), false, ()=> addAction.ModifyGraphDataAction(ViewModel.Model));
            }
            m_AddPropertyMenu.AddSeparator($"Keyword/");

            foreach (var nameToAddActionTuple in ViewModel.BuiltInKeywordNameToAddActionMap)
            {
                string builtInKeywordName = nameToAddActionTuple.Key;
                IGraphDataAction addAction = nameToAddActionTuple.Value;
                m_AddPropertyMenu.AddItem(new GUIContent($"Keyword/{builtInKeywordName}"), false, ()=> addAction.ModifyGraphDataAction(ViewModel.Model));
            }

            foreach (string disabledKeywordName in ViewModel.DisabledKeywordNameList)
            {
                m_AddPropertyMenu.AddDisabledItem(new GUIContent(disabledKeywordName));
            }
        }

        void ShowAddPropertyMenu()
        {
            m_AddPropertyMenu.ShowAsContext();
        }

        void OnMouseDownEvent(MouseDownEvent evt)
        {
            if (evt.clickCount == 2 && evt.button == (int)MouseButton.LeftMouse)
            {
                StartEditingPath();
                evt.PreventDefault();
            }
        }

        void StartEditingPath()
        {
            m_SubTitleLabel.visible = false;
            m_PathLabelTextField.visible = true;
            m_PathLabelTextField.value = m_SubTitleLabel.text;
            m_PathLabelTextField.Q("unity-text-input").Focus();
            m_PathLabelTextField.SelectAll();
        }

        void OnPathTextFieldKeyPressed(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Escape:
                    m_EditPathCancelled = true;
                    m_PathLabelTextField.Q("unity-text-input").Blur();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    m_PathLabelTextField.Q("unity-text-input").Blur();
                    break;
                default:
                    break;
            }
        }

        void OnEditPathTextFinished()
        {
            m_SubTitleLabel.visible = true;
            m_PathLabelTextField.visible = false;

            var newPath = m_PathLabelTextField.text;
            if (!m_EditPathCancelled && (newPath != m_SubTitleLabel.text))
            {
                newPath = BlackboardUtils.SanitizePath(newPath);
            }

            var pathChangeAction = new ChangeGraphPathAction();
            pathChangeAction.NewGraphPath = newPath;
            // Request graph path change action
            ViewModel.RequestModelChangeAction(pathChangeAction);

            m_SubTitleLabel.text =  BlackboardUtils.FormatPath(newPath);
            m_EditPathCancelled = false;
        }
    }
}
