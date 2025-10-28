<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmInventory
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblItemName = New System.Windows.Forms.Label()
        Me.lblCategory = New System.Windows.Forms.Label()
        Me.lblQuantity = New System.Windows.Forms.Label()
        Me.lblCondition = New System.Windows.Forms.Label()
        Me.lblLocation = New System.Windows.Forms.Label()
        Me.txtItemName = New System.Windows.Forms.TextBox()
        Me.txtQuantity = New System.Windows.Forms.TextBox()
        Me.txtLocation = New System.Windows.Forms.TextBox()
        Me.txtCondition = New System.Windows.Forms.TextBox()
        Me.txtCategory = New System.Windows.Forms.TextBox()
        Me.cmbCategory = New System.Windows.Forms.ComboBox()
        Me.cmbCondition = New System.Windows.Forms.ComboBox()
        Me.cmbLocation = New System.Windows.Forms.ComboBox()
        Me.btnAdd = New System.Windows.Forms.Button()
        Me.btnUpdate = New System.Windows.Forms.Button()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.btnSearch = New System.Windows.Forms.Button()
        Me.lblSearch = New System.Windows.Forms.Label()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.dgvInventory = New System.Windows.Forms.DataGridView()
        Me.btnBack = New System.Windows.Forms.Button()
        CType(Me.dgvInventory, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblItemName
        '
        Me.lblItemName.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblItemName.Location = New System.Drawing.Point(40, 80)
        Me.lblItemName.Name = "lblItemName"
        Me.lblItemName.Size = New System.Drawing.Size(100, 25)
        Me.lblItemName.TabIndex = 0
        Me.lblItemName.Text = "Item Name:"
        '
        'lblCategory
        '
        Me.lblCategory.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCategory.Location = New System.Drawing.Point(400, 80)
        Me.lblCategory.Name = "lblCategory"
        Me.lblCategory.Size = New System.Drawing.Size(80, 25)
        Me.lblCategory.TabIndex = 1
        Me.lblCategory.Text = "Category:"
        '
        'lblQuantity
        '
        Me.lblQuantity.Location = New System.Drawing.Point(40, 120)
        Me.lblQuantity.Name = "lblQuantity"
        Me.lblQuantity.Size = New System.Drawing.Size(100, 25)
        Me.lblQuantity.TabIndex = 2
        Me.lblQuantity.Text = "Quantity:"
        '
        'lblCondition
        '
        Me.lblCondition.AutoSize = True
        Me.lblCondition.Location = New System.Drawing.Point(260, 120)
        Me.lblCondition.Name = "lblCondition"
        Me.lblCondition.Size = New System.Drawing.Size(63, 15)
        Me.lblCondition.TabIndex = 3
        Me.lblCondition.Text = "Condition:"
        '
        'lblLocation
        '
        Me.lblLocation.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblLocation.Location = New System.Drawing.Point(600, 120)
        Me.lblLocation.Name = "lblLocation"
        Me.lblLocation.Size = New System.Drawing.Size(80, 25)
        Me.lblLocation.TabIndex = 4
        Me.lblLocation.Text = "Location:"
        '
        'txtItemName
        '
        Me.txtItemName.Location = New System.Drawing.Point(133, 80)
        Me.txtItemName.Name = "txtItemName"
        Me.txtItemName.Size = New System.Drawing.Size(200, 23)
        Me.txtItemName.TabIndex = 5
        '
        'txtQuantity
        '
        Me.txtQuantity.Location = New System.Drawing.Point(140, 120)
        Me.txtQuantity.Name = "txtQuantity"
        Me.txtQuantity.Size = New System.Drawing.Size(80, 23)
        Me.txtQuantity.TabIndex = 6
        '
        'txtLocation
        '
        Me.txtLocation.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLocation.Location = New System.Drawing.Point(680, 120)
        Me.txtLocation.Name = "txtLocation"
        Me.txtLocation.Size = New System.Drawing.Size(200, 23)
        Me.txtLocation.TabIndex = 7
        Me.txtLocation.Visible = False
        '
        'txtCondition
        '
        Me.txtCondition.Location = New System.Drawing.Point(350, 120)
        Me.txtCondition.Name = "txtCondition"
        Me.txtCondition.Size = New System.Drawing.Size(200, 23)
        Me.txtCondition.TabIndex = 8
        Me.txtCondition.Visible = False
        '
        'txtCategory
        '
        Me.txtCategory.Location = New System.Drawing.Point(480, 80)
        Me.txtCategory.Name = "txtCategory"
        Me.txtCategory.Size = New System.Drawing.Size(200, 23)
        Me.txtCategory.TabIndex = 9
        Me.txtCategory.Visible = False
        '
        'cmbCategory
        '
        Me.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbCategory.Font = New System.Drawing.Font("Segoe UI", 9.75!)
        Me.cmbCategory.FormattingEnabled = True
        Me.cmbCategory.Location = New System.Drawing.Point(480, 80)
        Me.cmbCategory.Name = "cmbCategory"
        Me.cmbCategory.Size = New System.Drawing.Size(200, 25)
        Me.cmbCategory.TabIndex = 24
        '
        'cmbCondition
        '
        Me.cmbCondition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbCondition.Font = New System.Drawing.Font("Segoe UI", 9.75!)
        Me.cmbCondition.FormattingEnabled = True
        Me.cmbCondition.Location = New System.Drawing.Point(350, 120)
        Me.cmbCondition.Name = "cmbCondition"
        Me.cmbCondition.Size = New System.Drawing.Size(200, 25)
        Me.cmbCondition.TabIndex = 25
        '
        'cmbLocation
        '
        Me.cmbLocation.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmbLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbLocation.Font = New System.Drawing.Font("Segoe UI", 9.75!)
        Me.cmbLocation.FormattingEnabled = True
        Me.cmbLocation.Location = New System.Drawing.Point(680, 120)
        Me.cmbLocation.Name = "cmbLocation"
        Me.cmbLocation.Size = New System.Drawing.Size(200, 25)
        Me.cmbLocation.TabIndex = 26
        '
        'btnAdd
        '
        Me.btnAdd.BackColor = System.Drawing.Color.SteelBlue
        Me.btnAdd.ForeColor = System.Drawing.Color.White
        Me.btnAdd.Location = New System.Drawing.Point(140, 170)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(90, 35)
        Me.btnAdd.TabIndex = 10
        Me.btnAdd.Text = "Add"
        Me.btnAdd.UseVisualStyleBackColor = False
        '
        'btnUpdate
        '
        Me.btnUpdate.BackColor = System.Drawing.Color.SteelBlue
        Me.btnUpdate.ForeColor = System.Drawing.Color.White
        Me.btnUpdate.Location = New System.Drawing.Point(240, 170)
        Me.btnUpdate.Name = "btnUpdate"
        Me.btnUpdate.Size = New System.Drawing.Size(90, 35)
        Me.btnUpdate.TabIndex = 11
        Me.btnUpdate.Text = "Update"
        Me.btnUpdate.UseVisualStyleBackColor = False
        '
        'btnClear
        '
        Me.btnClear.BackColor = System.Drawing.Color.SteelBlue
        Me.btnClear.ForeColor = System.Drawing.Color.White
        Me.btnClear.Location = New System.Drawing.Point(340, 170)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(90, 35)
        Me.btnClear.TabIndex = 12
        Me.btnClear.Text = "Clear"
        Me.btnClear.UseVisualStyleBackColor = False
        '
        'btnRefresh
        '
        Me.btnRefresh.BackColor = System.Drawing.Color.SteelBlue
        Me.btnRefresh.ForeColor = System.Drawing.Color.White
        Me.btnRefresh.Location = New System.Drawing.Point(440, 170)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(90, 35)
        Me.btnRefresh.TabIndex = 13
        Me.btnRefresh.Text = "Refresh"
        Me.btnRefresh.UseVisualStyleBackColor = False
        '
        'btnSearch
        '
        Me.btnSearch.BackColor = System.Drawing.Color.SteelBlue
        Me.btnSearch.ForeColor = System.Drawing.Color.White
        Me.btnSearch.Location = New System.Drawing.Point(380, 230)
        Me.btnSearch.Name = "btnSearch"
        Me.btnSearch.Size = New System.Drawing.Size(90, 30)
        Me.btnSearch.TabIndex = 15
        Me.btnSearch.Text = "Search"
        Me.btnSearch.UseVisualStyleBackColor = False
        '
        'lblSearch
        '
        Me.lblSearch.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSearch.Location = New System.Drawing.Point(40, 230)
        Me.lblSearch.Name = "lblSearch"
        Me.lblSearch.Size = New System.Drawing.Size(80, 25)
        Me.lblSearch.TabIndex = 16
        Me.lblSearch.Text = "Search:"
        '
        'txtSearch
        '
        Me.txtSearch.Location = New System.Drawing.Point(120, 230)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(250, 23)
        Me.txtSearch.TabIndex = 17
        '
        'dgvInventory
        '
        Me.dgvInventory.AllowUserToAddRows = False
        Me.dgvInventory.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvInventory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvInventory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvInventory.Location = New System.Drawing.Point(40, 280)
        Me.dgvInventory.Name = "dgvInventory"
        Me.dgvInventory.ReadOnly = True
        Me.dgvInventory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvInventory.Size = New System.Drawing.Size(900, 300)
        Me.dgvInventory.TabIndex = 18
        '
        'btnBack
        '
        Me.btnBack.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(120, Byte), Integer), CType(CType(215, Byte), Integer))
        Me.btnBack.FlatAppearance.BorderSize = 0
        Me.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnBack.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.btnBack.ForeColor = System.Drawing.Color.White
        Me.btnBack.Location = New System.Drawing.Point(20, 20)
        Me.btnBack.Name = "btnBack"
        Me.btnBack.Size = New System.Drawing.Size(160, 35)
        Me.btnBack.TabIndex = 19
        Me.btnBack.Text = "← Back to Main Menu"
        Me.btnBack.UseVisualStyleBackColor = False
        '
        'frmInventory
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.WhiteSmoke
        Me.ClientSize = New System.Drawing.Size(984, 611)
        Me.Controls.Add(Me.btnBack)
        Me.Controls.Add(Me.dgvInventory)
        Me.Controls.Add(Me.txtSearch)
        Me.Controls.Add(Me.lblSearch)
        Me.Controls.Add(Me.btnSearch)
        Me.Controls.Add(Me.btnRefresh)
        Me.Controls.Add(Me.btnClear)
        Me.Controls.Add(Me.btnUpdate)
        Me.Controls.Add(Me.btnAdd)
        Me.Controls.Add(Me.cmbCategory)
        Me.Controls.Add(Me.cmbCondition)
        Me.Controls.Add(Me.cmbLocation)
        Me.Controls.Add(Me.txtCategory)
        Me.Controls.Add(Me.txtCondition)
        Me.Controls.Add(Me.txtLocation)
        Me.Controls.Add(Me.txtQuantity)
        Me.Controls.Add(Me.txtItemName)
        Me.Controls.Add(Me.lblLocation)
        Me.Controls.Add(Me.lblCondition)
        Me.Controls.Add(Me.lblQuantity)
        Me.Controls.Add(Me.lblCategory)
        Me.Controls.Add(Me.lblItemName)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.MaximizeBox = True
        Me.MinimumSize = New System.Drawing.Size(1000, 650)
        Me.Name = "frmInventory"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Inventory Management"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        CType(Me.dgvInventory, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblItemName As Label
    Friend WithEvents lblCategory As Label
    Friend WithEvents lblQuantity As Label
    Friend WithEvents lblCondition As Label
    Friend WithEvents lblLocation As Label
    Friend WithEvents txtItemName As TextBox
    Friend WithEvents txtQuantity As TextBox
    Friend WithEvents txtLocation As TextBox
    Friend WithEvents txtCondition As TextBox
    Friend WithEvents txtCategory As TextBox
    Friend WithEvents btnAdd As Button
    Friend WithEvents btnUpdate As Button
    Friend WithEvents btnClear As Button
    Friend WithEvents btnRefresh As Button
    Friend WithEvents btnSearch As Button
    Friend WithEvents lblSearch As Label
    Friend WithEvents txtSearch As TextBox
    Friend WithEvents dgvInventory As DataGridView
    Friend WithEvents btnBack As Button
    Friend WithEvents cmbCategory As ComboBox
    Friend WithEvents cmbCondition As ComboBox
    Friend WithEvents cmbLocation As ComboBox
End Class