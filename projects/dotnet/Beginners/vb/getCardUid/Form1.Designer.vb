<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.lblCardUid = New System.Windows.Forms.Label()
        Me.label6 = New System.Windows.Forms.Label()
        Me.lblCardAtr = New System.Windows.Forms.Label()
        Me.label3 = New System.Windows.Forms.Label()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.label2 = New System.Windows.Forms.Label()
        Me.cbReaders = New System.Windows.Forms.ComboBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'btnRefresh
        '
        Me.btnRefresh.Image = CType(resources.GetObject("btnRefresh.Image"), System.Drawing.Image)
        Me.btnRefresh.Location = New System.Drawing.Point(466, 4)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(26, 30)
        Me.btnRefresh.TabIndex = 17
        Me.btnRefresh.UseVisualStyleBackColor = True
        '
        'lblCardUid
        '
        Me.lblCardUid.AutoSize = True
        Me.lblCardUid.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCardUid.ForeColor = System.Drawing.Color.DarkOliveGreen
        Me.lblCardUid.Location = New System.Drawing.Point(148, 102)
        Me.lblCardUid.Name = "lblCardUid"
        Me.lblCardUid.Size = New System.Drawing.Size(140, 13)
        Me.lblCardUid.TabIndex = 16
        Me.lblCardUid.Text = "XX XX XX XX XX XX XX XX"
        '
        'label6
        '
        Me.label6.AutoSize = True
        Me.label6.Location = New System.Drawing.Point(9, 103)
        Me.label6.Name = "label6"
        Me.label6.Size = New System.Drawing.Size(61, 13)
        Me.label6.TabIndex = 15
        Me.label6.Text = "Card's UID:"
        '
        'lblCardAtr
        '
        Me.lblCardAtr.AutoSize = True
        Me.lblCardAtr.Location = New System.Drawing.Point(148, 74)
        Me.lblCardAtr.Name = "lblCardAtr"
        Me.lblCardAtr.Size = New System.Drawing.Size(344, 13)
        Me.lblCardAtr.TabIndex = 14
        Me.lblCardAtr.Text = "XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX XX"
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Location = New System.Drawing.Point(9, 72)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(64, 13)
        Me.label3.TabIndex = 13
        Me.label3.Text = "Card's ATR:"
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Location = New System.Drawing.Point(148, 46)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(109, 13)
        Me.lblStatus.TabIndex = 12
        Me.lblStatus.Text = "Reader current status"
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(9, 47)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(112, 13)
        Me.label2.TabIndex = 11
        Me.label2.Text = "Reader current status:"
        '
        'cbReaders
        '
        Me.cbReaders.FormattingEnabled = True
        Me.cbReaders.Location = New System.Drawing.Point(148, 10)
        Me.cbReaders.Name = "cbReaders"
        Me.cbReaders.Size = New System.Drawing.Size(312, 21)
        Me.cbReaders.TabIndex = 10
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(9, 13)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(91, 13)
        Me.label1.TabIndex = 9
        Me.label1.Text = "Available readers:"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(509, 135)
        Me.Controls.Add(Me.btnRefresh)
        Me.Controls.Add(Me.lblCardUid)
        Me.Controls.Add(Me.label6)
        Me.Controls.Add(Me.lblCardAtr)
        Me.Controls.Add(Me.label3)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.label2)
        Me.Controls.Add(Me.cbReaders)
        Me.Controls.Add(Me.label1)
        Me.Name = "Form1"
        Me.Text = "Get card's UID"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private WithEvents btnRefresh As Button
    Private WithEvents lblCardUid As Label
    Private WithEvents label6 As Label
    Private WithEvents lblCardAtr As Label
    Private WithEvents label3 As Label
    Private WithEvents lblStatus As Label
    Private WithEvents label2 As Label
    Private WithEvents cbReaders As ComboBox
    Private WithEvents label1 As Label
End Class
