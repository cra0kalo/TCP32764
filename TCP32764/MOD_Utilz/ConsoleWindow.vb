Imports System.Text



''' <summary>
''' This class acts as the ConsoleWindow output
''' </summary>
''' 
Class ConsoleWindow
    Inherits TextBox



    Public Sub New()

        'Setup box
        Me.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.ReadOnly = True
        Me.Multiline = True

    End Sub

    Friend Sub PrintLine(msg As String)

        If Me.Text <> String.Empty Then
            Me.Text = Me.Text + System.Environment.NewLine + msg
        Else
            Me.Text = msg
        End If


        'Scroll as we write
        Me.SelectionStart = Me.Text.Length
        Me.ScrollToCaret()


    End Sub



    Friend Sub cls()
        Me.Text = String.Empty
    End Sub

    Friend Sub ScrollAlign()
        Me.SelectionStart = Me.Text.Length
        Me.ScrollToCaret()
    End Sub

    Friend Sub UnSelectTextWindowText()
        Me.Focus()
        Me.SelectionStart = Me.Text.Length
    End Sub

End Class
