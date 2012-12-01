<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FolderPackager
  Inherits System.Windows.Forms.Form

  'Form overrides dispose to clean up the component list.
  <System.Diagnostics.DebuggerNonUserCode()> _
  Protected Overrides Sub Dispose(ByVal disposing As Boolean)
    Try
      If disposing AndAlso components IsNot Nothing Then
        components.Dispose()
      End If
    Finally
      MyBase.Dispose(disposing)
    End Try
  End Sub

  'Required by the Windows Form Designer
  Private components As System.ComponentModel.IContainer

  'NOTE: The following procedure is required by the Windows Form Designer
  'It can be modified using the Windows Form Designer.  
  'Do not modify it using the code editor.
  <System.Diagnostics.DebuggerStepThrough()> _
  Private Sub InitializeComponent()
    Me.pnlTop = New System.Windows.Forms.Panel()
    Me.txtFileGroup = New System.Windows.Forms.TextBox()
    Me.lblFileGroup = New System.Windows.Forms.Label()
    Me.lblCollectionUuid = New System.Windows.Forms.Label()
    Me.btnCheckCollection = New System.Windows.Forms.Button()
    Me.updnCollectionID = New System.Windows.Forms.NumericUpDown()
    Me.cmbStartingFolderLevel = New System.Windows.Forms.ComboBox()
    Me.btnDestinationFolder = New System.Windows.Forms.Button()
    Me.btnSourceFolder = New System.Windows.Forms.Button()
    Me.txtDestinationFolder = New System.Windows.Forms.TextBox()
    Me.txtSourceFolder = New System.Windows.Forms.TextBox()
    Me.lblStartingFolderLevel = New System.Windows.Forms.Label()
    Me.lblCollectionId = New System.Windows.Forms.Label()
    Me.lblDestinationFolder = New System.Windows.Forms.Label()
    Me.lblSourceFolder = New System.Windows.Forms.Label()
    Me.fldrSourceFolder = New System.Windows.Forms.FolderBrowserDialog()
    Me.fldrDestinationFolder = New System.Windows.Forms.FolderBrowserDialog()
    Me.btnStart = New System.Windows.Forms.Button()
    Me.btnClose = New System.Windows.Forms.Button()
    Me.txtOut = New System.Windows.Forms.TextBox()
    Me.btnStop = New System.Windows.Forms.Button()
    Me.pnlTop.SuspendLayout()
    CType(Me.updnCollectionID, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.SuspendLayout()
    '
    'pnlTop
    '
    Me.pnlTop.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.pnlTop.Controls.Add(Me.txtFileGroup)
    Me.pnlTop.Controls.Add(Me.lblFileGroup)
    Me.pnlTop.Controls.Add(Me.lblCollectionUuid)
    Me.pnlTop.Controls.Add(Me.btnCheckCollection)
    Me.pnlTop.Controls.Add(Me.updnCollectionID)
    Me.pnlTop.Controls.Add(Me.cmbStartingFolderLevel)
    Me.pnlTop.Controls.Add(Me.btnDestinationFolder)
    Me.pnlTop.Controls.Add(Me.btnSourceFolder)
    Me.pnlTop.Controls.Add(Me.txtDestinationFolder)
    Me.pnlTop.Controls.Add(Me.txtSourceFolder)
    Me.pnlTop.Controls.Add(Me.lblStartingFolderLevel)
    Me.pnlTop.Controls.Add(Me.lblCollectionId)
    Me.pnlTop.Controls.Add(Me.lblDestinationFolder)
    Me.pnlTop.Controls.Add(Me.lblSourceFolder)
    Me.pnlTop.Location = New System.Drawing.Point(12, 12)
    Me.pnlTop.Name = "pnlTop"
    Me.pnlTop.Size = New System.Drawing.Size(819, 153)
    Me.pnlTop.TabIndex = 0
    '
    'txtFileGroup
    '
    Me.txtFileGroup.Location = New System.Drawing.Point(102, 118)
    Me.txtFileGroup.Name = "txtFileGroup"
    Me.txtFileGroup.Size = New System.Drawing.Size(176, 20)
    Me.txtFileGroup.TabIndex = 15
    '
    'lblFileGroup
    '
    Me.lblFileGroup.AutoSize = True
    Me.lblFileGroup.Location = New System.Drawing.Point(40, 121)
    Me.lblFileGroup.Name = "lblFileGroup"
    Me.lblFileGroup.Size = New System.Drawing.Size(55, 13)
    Me.lblFileGroup.TabIndex = 14
    Me.lblFileGroup.Text = "File Group"
    '
    'lblCollectionUuid
    '
    Me.lblCollectionUuid.AutoSize = True
    Me.lblCollectionUuid.Location = New System.Drawing.Point(284, 86)
    Me.lblCollectionUuid.Name = "lblCollectionUuid"
    Me.lblCollectionUuid.Size = New System.Drawing.Size(0, 13)
    Me.lblCollectionUuid.TabIndex = 13
    '
    'btnCheckCollection
    '
    Me.btnCheckCollection.Location = New System.Drawing.Point(165, 81)
    Me.btnCheckCollection.Name = "btnCheckCollection"
    Me.btnCheckCollection.Size = New System.Drawing.Size(113, 23)
    Me.btnCheckCollection.TabIndex = 12
    Me.btnCheckCollection.Text = "Check Collection..."
    Me.btnCheckCollection.UseVisualStyleBackColor = True
    '
    'updnCollectionID
    '
    Me.updnCollectionID.Location = New System.Drawing.Point(102, 84)
    Me.updnCollectionID.Name = "updnCollectionID"
    Me.updnCollectionID.Size = New System.Drawing.Size(45, 20)
    Me.updnCollectionID.TabIndex = 11
    '
    'cmbStartingFolderLevel
    '
    Me.cmbStartingFolderLevel.FormattingEnabled = True
    Me.cmbStartingFolderLevel.Items.AddRange(New Object() {"1", "2", "3", "4"})
    Me.cmbStartingFolderLevel.Location = New System.Drawing.Point(748, 81)
    Me.cmbStartingFolderLevel.Name = "cmbStartingFolderLevel"
    Me.cmbStartingFolderLevel.Size = New System.Drawing.Size(34, 21)
    Me.cmbStartingFolderLevel.TabIndex = 10
    Me.cmbStartingFolderLevel.Text = "1"
    '
    'btnDestinationFolder
    '
    Me.btnDestinationFolder.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.btnDestinationFolder.Location = New System.Drawing.Point(788, 46)
    Me.btnDestinationFolder.Name = "btnDestinationFolder"
    Me.btnDestinationFolder.Size = New System.Drawing.Size(28, 23)
    Me.btnDestinationFolder.TabIndex = 9
    Me.btnDestinationFolder.Text = "..."
    Me.btnDestinationFolder.UseVisualStyleBackColor = True
    '
    'btnSourceFolder
    '
    Me.btnSourceFolder.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.btnSourceFolder.Location = New System.Drawing.Point(788, 21)
    Me.btnSourceFolder.Name = "btnSourceFolder"
    Me.btnSourceFolder.Size = New System.Drawing.Size(28, 23)
    Me.btnSourceFolder.TabIndex = 8
    Me.btnSourceFolder.Text = "..."
    Me.btnSourceFolder.UseVisualStyleBackColor = True
    '
    'txtDestinationFolder
    '
    Me.txtDestinationFolder.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtDestinationFolder.Location = New System.Drawing.Point(102, 48)
    Me.txtDestinationFolder.Name = "txtDestinationFolder"
    Me.txtDestinationFolder.Size = New System.Drawing.Size(680, 20)
    Me.txtDestinationFolder.TabIndex = 5
    Me.txtDestinationFolder.Text = "\\libgrsurya\medusa_staging"
    '
    'txtSourceFolder
    '
    Me.txtSourceFolder.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtSourceFolder.Location = New System.Drawing.Point(102, 21)
    Me.txtSourceFolder.Name = "txtSourceFolder"
    Me.txtSourceFolder.Size = New System.Drawing.Size(680, 20)
    Me.txtSourceFolder.TabIndex = 4
    Me.txtSourceFolder.Text = "\\libgrsurya\medusa_staging"
    '
    'lblStartingFolderLevel
    '
    Me.lblStartingFolderLevel.AutoSize = True
    Me.lblStartingFolderLevel.Location = New System.Drawing.Point(638, 86)
    Me.lblStartingFolderLevel.Name = "lblStartingFolderLevel"
    Me.lblStartingFolderLevel.Size = New System.Drawing.Size(104, 13)
    Me.lblStartingFolderLevel.TabIndex = 3
    Me.lblStartingFolderLevel.Text = "Starting Folder Level"
    '
    'lblCollectionId
    '
    Me.lblCollectionId.AutoSize = True
    Me.lblCollectionId.Location = New System.Drawing.Point(31, 86)
    Me.lblCollectionId.Name = "lblCollectionId"
    Me.lblCollectionId.Size = New System.Drawing.Size(65, 13)
    Me.lblCollectionId.TabIndex = 2
    Me.lblCollectionId.Text = "Collection Id"
    '
    'lblDestinationFolder
    '
    Me.lblDestinationFolder.AutoSize = True
    Me.lblDestinationFolder.Location = New System.Drawing.Point(4, 55)
    Me.lblDestinationFolder.Name = "lblDestinationFolder"
    Me.lblDestinationFolder.Size = New System.Drawing.Size(92, 13)
    Me.lblDestinationFolder.TabIndex = 1
    Me.lblDestinationFolder.Text = "Destination Folder"
    '
    'lblSourceFolder
    '
    Me.lblSourceFolder.AutoSize = True
    Me.lblSourceFolder.Location = New System.Drawing.Point(23, 26)
    Me.lblSourceFolder.Name = "lblSourceFolder"
    Me.lblSourceFolder.Size = New System.Drawing.Size(73, 13)
    Me.lblSourceFolder.TabIndex = 0
    Me.lblSourceFolder.Text = "Source Folder"
    '
    'fldrSourceFolder
    '
    Me.fldrSourceFolder.Description = "Select the source folder containing the files to add to the submission package."
    Me.fldrSourceFolder.ShowNewFolderButton = False
    '
    'fldrDestinationFolder
    '
    Me.fldrDestinationFolder.Description = "Select the destination folder that will contain the packaged files."
    '
    'btnStart
    '
    Me.btnStart.Location = New System.Drawing.Point(12, 195)
    Me.btnStart.Name = "btnStart"
    Me.btnStart.Size = New System.Drawing.Size(95, 23)
    Me.btnStart.TabIndex = 8
    Me.btnStart.Text = "Start Packaging"
    Me.btnStart.UseVisualStyleBackColor = True
    '
    'btnClose
    '
    Me.btnClose.Location = New System.Drawing.Point(756, 195)
    Me.btnClose.Name = "btnClose"
    Me.btnClose.Size = New System.Drawing.Size(75, 23)
    Me.btnClose.TabIndex = 9
    Me.btnClose.Text = "Close"
    Me.btnClose.UseVisualStyleBackColor = True
    '
    'txtOut
    '
    Me.txtOut.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtOut.Location = New System.Drawing.Point(13, 224)
    Me.txtOut.Multiline = True
    Me.txtOut.Name = "txtOut"
    Me.txtOut.Size = New System.Drawing.Size(818, 420)
    Me.txtOut.TabIndex = 10
    '
    'btnStop
    '
    Me.btnStop.Location = New System.Drawing.Point(114, 195)
    Me.btnStop.Name = "btnStop"
    Me.btnStop.Size = New System.Drawing.Size(94, 23)
    Me.btnStop.TabIndex = 11
    Me.btnStop.Text = "Stop Packaging"
    Me.btnStop.UseVisualStyleBackColor = True
    '
    'FolderPackager
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(843, 656)
    Me.Controls.Add(Me.btnStop)
    Me.Controls.Add(Me.txtOut)
    Me.Controls.Add(Me.btnClose)
    Me.Controls.Add(Me.btnStart)
    Me.Controls.Add(Me.pnlTop)
    Me.Name = "FolderPackager"
    Me.Text = "Medusa Folder Packager"
    Me.pnlTop.ResumeLayout(False)
    Me.pnlTop.PerformLayout()
    CType(Me.updnCollectionID, System.ComponentModel.ISupportInitialize).EndInit()
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents pnlTop As System.Windows.Forms.Panel
  Friend WithEvents lblDestinationFolder As System.Windows.Forms.Label
  Friend WithEvents lblSourceFolder As System.Windows.Forms.Label
  Friend WithEvents lblCollectionId As System.Windows.Forms.Label
  Friend WithEvents txtDestinationFolder As System.Windows.Forms.TextBox
  Friend WithEvents txtSourceFolder As System.Windows.Forms.TextBox
  Friend WithEvents lblStartingFolderLevel As System.Windows.Forms.Label
  Friend WithEvents fldrSourceFolder As System.Windows.Forms.FolderBrowserDialog
  Friend WithEvents btnSourceFolder As System.Windows.Forms.Button
  Friend WithEvents fldrDestinationFolder As System.Windows.Forms.FolderBrowserDialog
  Friend WithEvents btnDestinationFolder As System.Windows.Forms.Button
  Friend WithEvents cmbStartingFolderLevel As System.Windows.Forms.ComboBox
  Friend WithEvents updnCollectionID As System.Windows.Forms.NumericUpDown
  Friend WithEvents btnCheckCollection As System.Windows.Forms.Button
  Friend WithEvents btnStart As System.Windows.Forms.Button
  Friend WithEvents btnClose As System.Windows.Forms.Button
  Friend WithEvents txtOut As System.Windows.Forms.TextBox
  Friend WithEvents btnStop As System.Windows.Forms.Button
  Friend WithEvents lblCollectionUuid As System.Windows.Forms.Label
  Friend WithEvents txtFileGroup As System.Windows.Forms.TextBox
  Friend WithEvents lblFileGroup As System.Windows.Forms.Label

End Class
