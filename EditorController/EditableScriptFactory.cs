﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public class EditableScriptData
    {
        public EditableScriptData(Element editor)
        {
            DisplayString = editor.Fields.GetString("display");
            Category = editor.Fields.GetString("category");
            CreateString = editor.Fields.GetString("create");
            AdderDisplayString = editor.Fields.GetString("add");
        }

        public string DisplayString { get; private set; }
        public string Category { get; private set; }
        public string CreateString { get; private set; }
        public string AdderDisplayString { get; private set; }
    }

    internal class EditableScriptFactory
    {
        private ScriptFactory m_scriptFactory;
        private WorldModel m_worldModel;
        private Dictionary<string, EditableScriptData> m_scriptData = new Dictionary<string, EditableScriptData>();

        internal EditableScriptFactory(ScriptFactory factory, WorldModel worldModel)
        {
            m_scriptFactory = factory;
            m_worldModel = worldModel;

            foreach (Element editor in worldModel.Elements.GetElements(ElementType.Editor).Where(e => IsScriptEditor(e)))
            {
                string appliesTo = editor.Fields.GetString("appliesto");
                m_scriptData.Add(appliesTo, new EditableScriptData(editor));
            }
        }

        private bool IsScriptEditor(Element editor)
        {
            return !string.IsNullOrEmpty(editor.Fields.GetString("display"));
        }

        internal EditableScript CreateScript(string keyword, string parent)
        {
            return CreateScript(keyword, m_worldModel.Elements.Get(parent));
        }

        internal EditableScript CreateScript(string keyword, Element parent)
        {
            IScript script = m_scriptFactory.CreateScript(keyword);
            return CreateScript(script, parent);
        }

        internal EditableScript CreateScript(IScript script, Element parent)
        {
            EditableScript newScript = new EditableScript(script, parent);
            if (m_scriptData.ContainsKey(script.Keyword)) newScript.DisplayTemplate = m_scriptData[script.Keyword].DisplayString;
            return newScript;
        }

        internal IEnumerable<string> GetCategories()
        {
            List<string> result = new List<string>();
            foreach (EditableScriptData data in m_scriptData.Values)
            {
                if (!result.Contains(data.Category)) result.Add(data.Category);
            }
            return result;
        }

        internal Dictionary<string, EditableScriptData> ScriptData
        {
            get { return m_scriptData; }
        }
    }
}
