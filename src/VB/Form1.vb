Public Class Form1

    Const istruction_label = "Instruction number selected: "
    Const matrix_size = 4
    Dim matrix(matrix_size, matrix_size) As Integer
    Dim cursor_x As Integer = 0
    Dim cursor_y As Integer = 0
    Dim current_instruction = 0
    Dim step_size As Integer

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' get all available ports
        For Each port In System.IO.Ports.SerialPort.GetPortNames()
            ComboBox1.Items.Add(port)
        Next
        ' if no ports are available, disable the "connection to the port" button
        If ComboBox1.Items.Count = 0 Then
            Button1.Enabled = False
            ComboBox1.Enabled = False
        End If

        step_size = PictureBox1.Height / matrix_size

        ' use graphics for adding the matrix on the picture
        Dim bmp As Bitmap = New Bitmap(PictureBox1.Width, PictureBox1.Height)
        Dim g As Graphics = Graphics.FromImage(bmp)
        Dim pn As New Pen(Color.Gray)
        ' calculate size of each square
        ' draw matrix squares on picturebox
        For index As Integer = 0 To matrix_size - 1
            g.DrawLine(pn, New Point(index * step_size, 0), New Point(index * step_size, PictureBox1.Height))
            g.DrawLine(pn, New Point(0, index * step_size), New Point(PictureBox1.Width, index * step_size))
        Next
        PictureBox1.Image = bmp

    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click
        ' each time the picturebox is clicked, update the cursor position to the corresponding square
        ' remove old red cursor 
        DrawRectangleCursor(New Point(cursor_x, cursor_y), False)
        Dim pos As Point = PictureBox1.PointToClient(Cursor.Position)
        ' calculate new cursor position
        cursor_x = (pos.X \ step_size)
        cursor_y = (pos.Y \ step_size)
        ' draw the red rectangle to the new position
        DrawRectangleCursor(pos, True)
        ' update the instruction index label
        update_instruction_label()
    End Sub

    Private Sub DrawRectangleCursor(ByVal point As Point, ByVal visible As Boolean)
        ' draw the rectangle based on the x,y pressed position
        Dim bmp As Bitmap = PictureBox1.Image
        Dim g As Graphics = Graphics.FromImage(bmp)
        ' depending on the boolean visible, show or delete the cursor
        Dim pn As Pen = IIf(visible, New Pen(Color.Red), New Pen(Color.Gray))
        g.DrawRectangle(pn, cursor_x * step_size, cursor_y * step_size, step_size, step_size)
        PictureBox1.Image = bmp
    End Sub

    Private Sub set_cursor_to_zero()
        ' function to reset the position of the cursor to the first square
        ' delete the old red rectangle cursor
        DrawRectangleCursor(New Point(cursor_x, cursor_y), False)
        ' set the cursor coords to zero
        cursor_x = 0
        cursor_y = 0
        ' draw the new red rectangle cursor
        DrawRectangleCursor(New Point(cursor_x, cursor_y), True)
        ' update the label of the current instruction index
        update_instruction_label()
    End Sub

    Private Sub update_instruction_label()
        ' function to update the label of the current instruction index
        Label1.Text = istruction_label + (cursor_x + (cursor_y * matrix_size)).ToString
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ' if the type istruction is not selected, return
        If ComboBox2.SelectedIndex = -1 Then
            MsgBox("No Instruction type selected")
            Return
        End If
        ' if the type istruction is motor, but the motor index is not selected, return
        If ComboBox2.SelectedIndex = 0 And ComboBox3.SelectedIndex = -1 Then
            MsgBox("No motor action selected")
            Return
        End If

        Dim bmp As Bitmap = New Bitmap(PictureBox1.Image)
        Dim g As Graphics = Graphics.FromImage(bmp)

        ' clean the old information inside the current selected square
        g.FillRectangle(New System.Drawing.SolidBrush(Color.White), (cursor_x * step_size) + 1, (cursor_y * step_size) + 1, step_size - 1, step_size - 1)

        ' if the instruction type is motor, print all the information inside the current square
        If ComboBox2.SelectedIndex = 0 Then
            ' put "motor information" on the communication format the value of the instruction to send to the Arduino
            matrix(cursor_x, cursor_y) = (ComboBox3.SelectedIndex << 4) Or ((NumericUpDown1.Value - 1) << 1) Or 1
            ' print the information
            g.DrawString("Motors", New Font("Arial", 8), Brushes.Black, (cursor_x * step_size) + 10, (cursor_y * step_size) + 15)
            g.DrawString("n°" & NumericUpDown1.Value, New Font("Arial", 8), Brushes.Black, (cursor_x * step_size) + 20, (cursor_y * step_size) + 25)
            If ComboBox3.SelectedIndex = 1 Then
                g.DrawString("Distension", New Font("Arial", 8), Brushes.Black, ((cursor_x * step_size) + 1), ((cursor_y * step_size) + 35))
            Else
                g.DrawString("Bending", New Font("Arial", 8), Brushes.Black, ((cursor_x * step_size) + 5), ((cursor_y * step_size) + 35))
            End If
        Else
            ' put "wait information" based on the communication format the value of the instruction to send to the Arduino
            matrix(cursor_x, cursor_y) = ((NumericUpDown2.Value / 100) << 1)
            g.DrawString("Pause", New Font("Arial", 8), Brushes.Black, (cursor_x * step_size) + 10, ((cursor_y * step_size) + 15))
            g.DrawString(NumericUpDown2.Value & "ms", New Font("Arial", 8), Brushes.Black, ((cursor_x * step_size) + 10), ((cursor_y * step_size) + 25))
        End If

        ' update the image grid
        PictureBox1.Image = bmp
    End Sub

    Private Sub PictureBox1_DoubleClick(sender As Object, e As EventArgs) Handles PictureBox1.DoubleClick
        ' function used to clean the information of a specific square with a double click action
        Dim bmp As Bitmap = New Bitmap(PictureBox1.Image)
        Dim g As Graphics = Graphics.FromImage(bmp)
        g.FillRectangle(New System.Drawing.SolidBrush(Color.White), (cursor_x * step_size) + 1, (cursor_y * step_size) + 1, step_size - 1, step_size - 1)
        matrix(cursor_x, cursor_y) = 0
        PictureBox1.Image = bmp
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        ' enable/disable the inputs based on the selected information type
        If ComboBox2.SelectedIndex = 0 Then
            ' make the motor panel input enabled
            Panel3.Enabled = True
            Panel2.Enabled = False
        Else
            ' make the pause panel input enabled
            Panel3.Enabled = False
            Panel2.Enabled = True
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' if no port are selected, return
        If ComboBox1.SelectedIndex = -1 Then
            MsgBox("No port selected from the available ones")
            Return
        End If

        ' Configure the SerialPort
        SerialPort1.PortName = ComboBox1.SelectedItem.ToString
        SerialPort1.BaudRate = 9600
        SerialPort1.ReadTimeout = 5000
        SerialPort1.WriteTimeout = 5000
        ' Try to open the serial port and write 255 value on the buffer to initialize the communication
        Try
            SerialPort1.Open()
            SerialPort1.DiscardInBuffer()
            SerialPort1.Write(255)
            Button1.Enabled = False
            Button2.Enabled = True
            PictureBox1.Enabled = True
            GroupBox1.Enabled = True
            ' draw cursor and update the instruction index label
            DrawRectangleCursor(New Point(cursor_x, cursor_y), True)
            update_instruction_label()
        Catch ex As Exception
            MsgBox("Impossible to establish a connection with " & ComboBox1.SelectedItem.ToString)
        End Try
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim confermation As Byte
        Dim current_instruction As Integer = 0

        ' set the cursor to the first square to start the execution
        set_cursor_to_zero()

        ' disable the execution button while is executing the instructions
        Button2.Enabled = False

        ' iterate over each square to execute each instruction
        For i As Integer = 0 To ((matrix_size * matrix_size) - 1)
            ' remove the old cursor
            DrawRectangleCursor(New Point(cursor_x, cursor_y), False)
            ' calculate the position in the matrix based on the current index value
            cursor_x = (i Mod matrix_size)
            cursor_y = (i \ matrix_size)
            ' get the value from the matrix to send to Arduino
            current_instruction = matrix(cursor_x, cursor_y)
            ' draw the cursor in the new update position
            DrawRectangleCursor(New Point(cursor_x, cursor_y), True)
            update_instruction_label()

            ' if the current instruction is empty, stop the communication
            If current_instruction = 0 Then
                Exit For
            End If

            Try
                ' try to send the current instruction to the Arduino
                SerialPort1.Write(current_instruction)
                ' wait for the byte of successful execution
                confermation = SerialPort1.ReadByte()
            Catch ex As TimeoutException
                ' if no bytes have been received after the timeout, I terminate execution
                MsgBox("ERROR: Timeour error with Arduino")
                Exit For
            Catch ex As Exception
                ' manage generic execution communication errors
                MsgBox("ERROR: Communication error with Arduino")
                Exit For
            End Try

        Next

        ' enable the execution button and set the cursor on the first square
        Button2.Enabled = True
        set_cursor_to_zero()
    End Sub

End Class