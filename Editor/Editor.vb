﻿Public Class Editor

    Private WithEvents m_controller As EditorController
    Private m_elementEditors As Dictionary(Of String, ElementEditor)
    Private m_currentEditor As ElementEditor
    Private m_menu As AxeSoftware.Quest.Controls.Menu
    Private m_filename As String
    Private m_currentElement As String

    Public Event AddToRecent(ByVal filename As String, ByVal name As String)

    Public Sub Initialise(ByRef filename As String)
        m_filename = filename
        m_controller = New EditorController()
        m_controller.Initialise(filename)
        ctlTree.SetAvailableFilters(m_controller.AvailableFilters)
        SetUpToolbar()
        SetUpEditors()
        RaiseEvent AddToRecent(filename, m_controller.GameName)
    End Sub

    Public Sub SetMenu(ByVal menu As AxeSoftware.Quest.Controls.Menu)
        m_menu = menu
        menu.AddMenuClickHandler("save", AddressOf Save)
        menu.AddMenuClickHandler("saveas", AddressOf SaveAs)
        menu.AddMenuClickHandler("undo", AddressOf Undo)
        menu.AddMenuClickHandler("redo", AddressOf Redo)
    End Sub

    Private Sub SetUpToolbar()
        ctlToolbar.ResetToolbar()
        ctlToolbar.AddButtonHandler("save", AddressOf Save)
        ctlToolbar.AddButtonHandler("undo", AddressOf Undo)
        ctlToolbar.AddButtonHandler("redo", AddressOf Redo)
    End Sub

    Private Sub SetUpEditors()
        m_elementEditors = New Dictionary(Of String, ElementEditor)

        For Each editor As String In m_controller.GetAllEditorNames()
            AddEditor(editor)
        Next
    End Sub

    Private Sub AddEditor(ByVal name As String)
        ' Get an EditorDefinition from the EditorController, then pass it in to the ElementEditor so it can initialise its
        ' tabs and subcontrols.
        Dim editor As ElementEditor
        editor = New ElementEditor
        editor.Initialise(m_controller, m_controller.GetEditorDefinition(name))
        editor.Visible = False
        editor.Parent = splitMain.Panel2
        editor.Dock = DockStyle.Fill
        AddHandler editor.Dirty, AddressOf Editor_Dirty
        m_elementEditors.Add(name, editor)
    End Sub

    Private Sub Editor_Dirty()
        ctlToolbar.EnableUndo()
        ' TO DO: Set status saying game not saved
    End Sub

    Private Sub m_controller_AddedNode(ByVal key As String, ByVal text As String, ByVal parent As String, ByVal foreColor As System.Drawing.Color?, ByVal backColor As System.Drawing.Color?) Handles m_controller.AddedNode
        ctlTree.AddNode(key, text, parent, foreColor, backColor)
    End Sub

    Private Sub m_controller_BeginTreeUpdate() Handles m_controller.BeginTreeUpdate
        ctlTree.BeginUpdate()
    End Sub

    Private Sub m_controller_ClearTree() Handles m_controller.ClearTree
        ctlTree.Clear()
    End Sub

    Private Sub m_controller_ElementUpdated(ByVal sender As Object, ByVal e As EditorController.ElementUpdatedEventArgs) Handles m_controller.ElementUpdated
        If e.Element = m_currentElement Then
            m_currentEditor.UpdateField(e.Attribute, e.NewValue, e.IsUndo)
        End If
    End Sub

    Private Sub m_controller_EndTreeUpdate() Handles m_controller.EndTreeUpdate
        ctlTree.EndUpdate()
    End Sub

    Private Sub m_controller_UndoListUpdated(ByVal sender As Object, ByVal e As EditorController.UpdateUndoListEventArgs) Handles m_controller.UndoListUpdated
        ctlToolbar.UpdateUndoMenu(e.UndoList)
    End Sub

    Private Sub m_controller_RedoListUpdated(ByVal sender As Object, ByVal e As EditorController.UpdateUndoListEventArgs) Handles m_controller.RedoListUpdated
        ctlToolbar.UpdateRedoMenu(e.UndoList)
    End Sub

    Private Sub m_controller_ShowMessage(ByVal message As String) Handles m_controller.ShowMessage
        System.Windows.Forms.MessageBox.Show(message)
    End Sub

    Private Sub ctlTree_FiltersUpdated() Handles ctlTree.FiltersUpdated
        m_controller.UpdateFilterOptions(ctlTree.FilterSettings)
    End Sub

    Private Sub ctlTree_SelectionChanged(ByVal key As String) Handles ctlTree.SelectionChanged
        ' TO DO: Need to add the tree text as the second parameter so we get friendly name for "Verbs" etc. instead of the key
        ctlToolbar.AddHistory(key, key)
        ShowEditor(key)
    End Sub

    Private Sub ShowEditor(ByVal key As String)

        Dim editorName As String = m_controller.GetElementEditorName(key)
        Dim nextEditor As ElementEditor = m_elementEditors(editorName)

        nextEditor.Visible = True

        If Not m_currentEditor Is Nothing Then
            If Not m_currentEditor.Equals(nextEditor) Then
                m_currentEditor.Visible = False
            End If
        End If

        m_currentEditor = nextEditor

        m_currentElement = key
        m_currentEditor.Populate(m_controller.GetEditorData(key))
    End Sub

    Private Sub Save()
        ' TO DO: Save the currently selected control first

        If (m_filename.Length = 0) Then
            SaveAs()
        Else
            Save(m_filename)
        End If
    End Sub

    Private Sub SaveAs()
        ctlSaveFile.FileName = m_filename
        If ctlSaveFile.ShowDialog() = DialogResult.OK Then
            m_filename = ctlSaveFile.FileName
            Save(m_filename)
        End If
    End Sub

    Private Sub Save(ByVal filename As String)
        Try
            If Not m_currentEditor Is Nothing Then
                m_currentEditor.SaveData()
            End If
            System.IO.File.WriteAllText(filename, m_controller.Save())
        Catch ex As Exception
            MsgBox("Unable to save the file due to the following error:" + Environment.NewLine + Environment.NewLine + ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub ctlToolbar_HistoryClicked(ByVal Key As String) Handles ctlToolbar.HistoryClicked
        ctlTree.SetSelectedItem(Key)
        ShowEditor(Key)
    End Sub

    Private Sub Undo()
        If Not m_currentEditor Is Nothing Then
            m_currentEditor.SaveData()
            m_controller.Undo()
        End If
    End Sub

    Private Sub Redo()
        If Not m_currentEditor Is Nothing Then
            m_currentEditor.SaveData()
            m_controller.Redo()
        End If
    End Sub

    Private Sub ctlToolbar_SaveCurrentEditor() Handles ctlToolbar.SaveCurrentEditor
        If Not m_currentEditor Is Nothing Then
            m_currentEditor.SaveData()
        End If
    End Sub

    Private Sub ctlToolbar_UndoClicked(ByVal level As Integer) Handles ctlToolbar.UndoClicked
        m_controller.Undo(level)
    End Sub

    Private Sub ctlToolbar_RedoClicked(ByVal level As Integer) Handles ctlToolbar.RedoClicked
        m_controller.Redo(level)
    End Sub

    Private Sub ctlToolbar_UndoEnabled(ByVal enabled As Boolean) Handles ctlToolbar.UndoEnabled
        m_menu.MenuEnabled("undo") = enabled
    End Sub

    Private Sub ctlToolbar_RedoEnabled(ByVal enabled As Boolean) Handles ctlToolbar.RedoEnabled
        m_menu.MenuEnabled("redo") = enabled
    End Sub
End Class
