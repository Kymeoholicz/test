Imports System.Data.OleDb

Public Module DatabaseFixHelper

    ''' <summary>
    ''' Checks if the tblUsers table has the correct structure and fixes it if needed
    ''' </summary>
    Public Sub FixUserTableStructure()
        Try
            Using con As New OleDbConnection(DatabaseConfig.ConnectionString)
                con.Open()

                ' Get the schema of tblUsers
                Dim schema As DataTable = con.GetSchema("Columns", New String() {Nothing, Nothing, "tblUsers", Nothing})

                Dim hasCreatedBy As Boolean = False
                Dim hasPassword As Boolean = False
                Dim hasUserPassword As Boolean = False

                For Each row As DataRow In schema.Rows
                    Dim columnName As String = row("COLUMN_NAME").ToString()

                    If columnName.Equals("CreatedBy", StringComparison.OrdinalIgnoreCase) Then
                        hasCreatedBy = True
                    ElseIf columnName.Equals("Password", StringComparison.OrdinalIgnoreCase) Then
                        hasPassword = True
                    ElseIf columnName.Equals("UserPassword", StringComparison.OrdinalIgnoreCase) Then
                        hasUserPassword = True
                    End If
                Next

                ' Check if we need to fix anything
                If hasCreatedBy OrElse hasPassword Then
                    Dim message As String = "Database structure needs updating:" & vbCrLf & vbCrLf

                    If hasCreatedBy Then
                        message &= "- Found 'CreatedBy' field (not used)" & vbCrLf
                    End If

                    If hasPassword Then
                        message &= "- Found 'Password' field (reserved keyword)" & vbCrLf
                    End If

                    message &= vbCrLf & "Would you like to automatically fix the database structure?" & vbCrLf &
                               "(This will backup your data and recreate the table)"

                    Dim result = MessageBox.Show(message, "Database Update Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

                    If result = DialogResult.Yes Then
                        FixTableStructure(con)
                    Else
                        MessageBox.Show("Database not updated. You may encounter errors when adding users." & vbCrLf & vbCrLf &
                                      "To fix manually:" & vbCrLf &
                                      "1. Delete inventory.accdb" & vbCrLf &
                                      "2. Restart the application",
                                      "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    End If
                End If

            End Using
        Catch ex As Exception
            MessageBox.Show("Error checking database structure: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FixTableStructure(con As OleDbConnection)
        Try
            ' Step 1: Backup existing user data
            Dim userData As New DataTable()
            Using da As New OleDbDataAdapter("SELECT UserID, Username, [Password], [UserPassword], FullName, Email, UserRole, IsActive, DateCreated FROM tblUsers", con)
                Try
                    da.Fill(userData)
                Catch
                    ' Try alternative field name
                    userData.Clear()
                    Using da2 As New OleDbDataAdapter("SELECT UserID, Username, FullName, Email, UserRole, IsActive, DateCreated FROM tblUsers", con)
                        da2.Fill(userData)
                    End Using
                End Try
            End Using

            ' Step 2: Drop the old table
            Using cmd As New OleDbCommand("DROP TABLE tblUsers", con)
                cmd.ExecuteNonQuery()
            End Using

            ' Step 3: Create new table with correct structure
            Using cmd As New OleDbCommand("CREATE TABLE tblUsers (UserID AUTOINCREMENT PRIMARY KEY, Username TEXT(50), [UserPassword] TEXT(255), FullName TEXT(100), Email TEXT(100), UserRole TEXT(20), IsActive YESNO, DateCreated DATETIME)", con)
                cmd.ExecuteNonQuery()
            End Using

            ' Step 4: Restore data
            For Each row As DataRow In userData.Rows
                Dim passwordValue As String = ""

                ' Try to get password from either field
                If userData.Columns.Contains("UserPassword") AndAlso Not IsDBNull(row("UserPassword")) Then
                    passwordValue = row("UserPassword").ToString()
                ElseIf userData.Columns.Contains("Password") AndAlso Not IsDBNull(row("Password")) Then
                    passwordValue = row("Password").ToString()
                End If

                Using cmd As New OleDbCommand("INSERT INTO tblUsers (Username, [UserPassword], FullName, Email, UserRole, IsActive, DateCreated) VALUES (?,?,?,?,?,?,?)", con)
                    cmd.Parameters.Add("?", OleDbType.VarWChar, 50).Value = If(IsDBNull(row("Username")), "", row("Username").ToString())
                    cmd.Parameters.Add("?", OleDbType.VarWChar, 255).Value = passwordValue
                    cmd.Parameters.Add("?", OleDbType.VarWChar, 100).Value = If(IsDBNull(row("FullName")), "", row("FullName").ToString())
                    cmd.Parameters.Add("?", OleDbType.VarWChar, 100).Value = If(IsDBNull(row("Email")), "", row("Email").ToString())
                    cmd.Parameters.Add("?", OleDbType.VarWChar, 20).Value = If(IsDBNull(row("UserRole")), "User", row("UserRole").ToString())
                    cmd.Parameters.Add("?", OleDbType.Boolean).Value = If(IsDBNull(row("IsActive")), True, CBool(row("IsActive")))
                    cmd.Parameters.Add("?", OleDbType.Date).Value = If(IsDBNull(row("DateCreated")), Date.Now, CDate(row("DateCreated")))
                    cmd.ExecuteNonQuery()
                End Using
            Next

            MessageBox.Show("Database structure updated successfully!" & vbCrLf &
                          $"Restored {userData.Rows.Count} user(s).",
                          "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Error fixing database structure: " & ex.Message & vbCrLf & vbCrLf &
                          "Please delete inventory.accdb and restart the application.",
                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Module