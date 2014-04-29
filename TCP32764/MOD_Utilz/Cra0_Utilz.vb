Option Explicit On


'// We use some imports
Imports System.IO
Imports System.Runtime.InteropServices

Module Cra0_Utilz

#Region "Cra0 Utilz"

    Public Enum ByteOrder As Integer
        LittleEndian
        BigEndian
        UnknownEndian
    End Enum



    Public Function GetBytes_ASCII(ByVal inString As String) As Byte()
        Return System.Text.Encoding.ASCII.GetBytes(inString)
    End Function

    Public Function GetBytes_UTF8(ByVal inString As String) As Byte()
        Return System.Text.Encoding.UTF8.GetBytes(inString)
    End Function

    Public Function GetString_ASCII(ByVal byteArray As Byte()) As String
        Return System.Text.Encoding.ASCII.GetString(byteArray)
    End Function

    Public Function GetString_UTF8(ByVal byteArray As Byte()) As String
        Return System.Text.Encoding.UTF8.GetString(byteArray)
    End Function

    Public Sub WriteAscii(ByVal writer As BinaryWriter, ByVal asciiString As String)
        'Writes a ascii string with a null terminator hence "AsciiZ"
        writer.Write(System.Text.ASCIIEncoding.ASCII.GetBytes(asciiString))
    End Sub

    Public Sub WriteAsciiz(ByVal writer As BinaryWriter, ByVal asciiString As String)
        'Writes a ascii string with a null terminator hence "AsciiZ"
        writer.Write(System.Text.ASCIIEncoding.ASCII.GetBytes(asciiString))
        writer.Write(New Byte)
    End Sub

    Public Sub WriteUINT64(ByVal writer As BinaryWriter, ByVal value As UInt64)
        'Writes a 32bit value to a 64bit space
        writer.Write(CType(value, UInt64))

    End Sub

    Public Sub WriteNullByte(ByVal writer As BinaryWriter)
        Dim NullB As Byte = 0
        writer.Write(NullB)
    End Sub

    Public Sub WriteNullBytes(ByVal writer As BinaryWriter, ByVal count As Integer)
        Dim NullB As Byte = 0
        For i As Integer = 0 To (count - 1)
            writer.Write(NullB)
        Next
    End Sub


    Public Function ReadAsciiZArray(ByVal byteArray As Byte()) As List(Of String)
        Dim returnVal As New List(Of String)

        Using ms As New MemoryStream(byteArray)
            Dim br As New BinaryReader(ms)

            Do While ms.Position < ms.Length

                Dim str As String = String.Empty
                Try
                    'Read till Null terminator
                    Dim b As Byte
                    b = br.ReadByte()
                    Do While b <> 0
                        str &= ChrW(b)
                        b = br.ReadByte()
                    Loop
                Catch ex As Exception
                End Try
                returnVal.Add(str)
            Loop
        End Using

        Return returnVal
    End Function



    Public Function ReadBytes(ByVal reader As BinaryReader, ByVal fieldSize As Integer, ByVal byteOrder As ByteOrder) As Byte()
        Dim bytes(fieldSize - 1) As Byte
        If byteOrder = byteOrder.LittleEndian Then
            Return reader.ReadBytes(fieldSize)
        Else
            For i As Integer = fieldSize - 1 To 0 Step -1
                bytes(i) = reader.ReadByte()
            Next i
            Return bytes
        End If
    End Function



    Public Function ReadInt32(ByVal reader As BinaryReader, ByVal byteOrder As ByteOrder) As Integer
        If byteOrder = byteOrder.LittleEndian Then
            Return reader.ReadInt32()
        Else ' Big-Endian
            Return BitConverter.ToInt32(ReadBytes(reader, 4, byteOrder.BigEndian), 0)
        End If
    End Function

    Public Function ReadUInt32(ByVal reader As BinaryReader, ByVal byteOrder As ByteOrder) As UInteger
        If byteOrder = byteOrder.LittleEndian Then
            Return reader.ReadUInt32()
        Else ' Big-Endian
            Return BitConverter.ToUInt32(ReadBytes(reader, 4, byteOrder.BigEndian), 0)
        End If
    End Function

    Public Function ReadUInt64(ByVal reader As BinaryReader, ByVal byteOrder As ByteOrder) As ULong
        If byteOrder = byteOrder.LittleEndian Then
            Return reader.ReadUInt64()
        Else ' Big-Endian
            Return BitConverter.ToUInt64(ReadBytes(reader, 8, byteOrder.BigEndian), 0)
        End If
    End Function


    Public Function GetBytes(ByRef Inarray As Byte(), ByVal numofBytes As Integer, Optional offset As Integer = 0) As Byte()
        Dim ReturnBytes(numofBytes) As Byte
        Array.Copy(Inarray, offset, ReturnBytes, 0, ReturnBytes.Length)
        Return ReturnBytes
    End Function




    Public Sub WriteBytes(ByVal writer As BinaryWriter, ByVal data As Byte(), ByVal byteOrder As ByteOrder)

        If byteOrder = byteOrder.LittleEndian Then
            writer.Write(data)
        Else
            Dim byteData(data.Length - 1) As Byte
            Array.Copy(data, byteData, data.Length)
            Array.Reverse(byteData)
            writer.Write(byteData)
            byteData = Nothing
        End If
    End Sub


    Public Sub WriteInt32(ByVal writer As BinaryWriter, ByVal data As Int32, ByVal byteOrder As ByteOrder)
        If byteOrder = byteOrder.LittleEndian Then
            WriteBytes(writer, BitConverter.GetBytes(data), Cra0_Utilz.ByteOrder.LittleEndian)
        Else ' Big-Endian
            WriteBytes(writer, BitConverter.GetBytes(data), Cra0_Utilz.ByteOrder.BigEndian)
        End If
    End Sub

    Public Sub WriteUInt32(ByVal writer As BinaryWriter, ByVal data As UInt32, ByVal byteOrder As ByteOrder)
        If byteOrder = byteOrder.LittleEndian Then
            WriteBytes(writer, BitConverter.GetBytes(data), Cra0_Utilz.ByteOrder.LittleEndian)
        Else ' Big-Endian
            WriteBytes(writer, BitConverter.GetBytes(data), Cra0_Utilz.ByteOrder.BigEndian)
        End If
    End Sub

    Public Sub WriteInt64(ByVal writer As BinaryWriter, ByVal data As Int64, ByVal byteOrder As ByteOrder)
        If byteOrder = byteOrder.LittleEndian Then
            WriteBytes(writer, BitConverter.GetBytes(data), Cra0_Utilz.ByteOrder.LittleEndian)
        Else ' Big-Endian
            WriteBytes(writer, BitConverter.GetBytes(data), Cra0_Utilz.ByteOrder.BigEndian)
        End If
    End Sub

    Public Sub WriteUInt64(ByVal writer As BinaryWriter, ByVal data As UInt64, ByVal byteOrder As ByteOrder)
        If byteOrder = byteOrder.LittleEndian Then
            WriteBytes(writer, BitConverter.GetBytes(data), Cra0_Utilz.ByteOrder.LittleEndian)
        Else ' Big-Endian
            WriteBytes(writer, BitConverter.GetBytes(data), Cra0_Utilz.ByteOrder.BigEndian)
        End If
    End Sub

#End Region


End Module
