/********************************** Module Header **********************************\
Module Name:  Program.cs
Project:      CSMailslotServer
Copyright (c) Microsoft Corporation.
 
Mailslot is a mechanism for one-way inter-process communication in the local machine
or across the computers in the intranet. Any clients can store messages in a mailslot.
The creator of the slot, i.e. the server, retrieves the messages that are stored
there:
 
Client (GENERIC_WRITE) ---> Server (GENERIC_READ)
 
This code sample demonstrates calling CreateMailslot to create a mailslot named
"\\.\mailslot\SampleMailslot". The security attributes of the slot are customized to
allow Authenticated Users read and write access to the slot, and to allow the
Administrators group full access to it. The sample first creates such a mailslot,
then it reads and displays new messages in the slot when user presses ENTER in the
console.
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.
 
THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***********************************************************************************/

#region Using directives
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;
using System.ComponentModel;
#endregion


namespace SpringCard.LibCs.Windows
{
	public class MailSlotServer
	{

		public static SafeMailslotHandle Create(string mailSlotName)
		{
			SECURITY_ATTRIBUTES sa = null;
			sa = CreateMailslotSecurity();
			return MailSlot.CreateMailslot(mailSlotName, 0, MAILSLOT_WAIT_FOREVER, sa);
			
		}
//		
//		
//		public static SafeMailslotHandle MailSlotOpen(string mailSlotName)
//		{
//			return Traceability.MailSlot.CreateFile(
//				mailSlotName,                           // The name of the mailslot
//				FileDesiredAccess.GENERIC_WRITE,        // Write access
//				FileShareMode.FILE_SHARE_READ,          // Share mode
//				IntPtr.Zero,                            // Default security attributes
//				FileCreationDisposition.OPEN_EXISTING,  // Opens existing mailslot
//				0,                                      // No other attributes set
//				IntPtr.Zero                             // No template file
//			);
//		}
		
		
		/// <summary>
		/// The CreateMailslotSecurity function creates and initializes a new
		/// SECURITY_ATTRIBUTES object to allow Authenticated Users read and
		/// write access to a mailslot, and to allow the Administrators group full
		/// access to the mailslot.
		/// </summary>
		/// <returns>
		/// A SECURITY_ATTRIBUTES object that allows Authenticated Users read and
		/// write access to a mailslot, and allows the Administrators group full
		/// access to the mailslot.
		/// </returns>
		/// <see cref="http://msdn.microsoft.com/en-us/library/aa365600.aspx"/>
		
		static SECURITY_ATTRIBUTES CreateMailslotSecurity()
		{
			// Define the SDDL for the security descriptor.
			string sddl = "D:" +        // Discretionary ACL
				"(A;OICI;GRGW;;;AU)" +  // Allow read/write to authenticated users
				"(A;OICI;GA;;;BA)";     // Allow full control to administrators

			SafeLocalMemHandle pSecurityDescriptor = null;
			if (!MailSlot.ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, 1, out pSecurityDescriptor, IntPtr.Zero))
			{
				throw new Win32Exception();
			}

			SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
			sa.nLength = Marshal.SizeOf(sa);
			sa.lpSecurityDescriptor = pSecurityDescriptor;
			sa.bInheritHandle = false;
			return sa;
		}


		#region Native API Signatures and Types

		/// <summary>
		/// Desired Access of File/Device
		/// </summary>
		[Flags]
		public enum FileDesiredAccess : uint
		{
			GENERIC_READ = 0x80000000,
			GENERIC_WRITE = 0x40000000,
			GENERIC_EXECUTE = 0x20000000,
			GENERIC_ALL = 0x10000000
		}

		/// <summary>
		/// File share mode
		/// </summary>
		[Flags]
		public enum FileShareMode : uint
		{
			Zero = 0x00000000,  // No sharing
			FILE_SHARE_DELETE = 0x00000004,
			FILE_SHARE_READ = 0x00000001,
			FILE_SHARE_WRITE = 0x00000002
		}

		/// <summary>
		/// File Creation Disposition
		/// </summary>
		public enum FileCreationDisposition : uint
		{
			CREATE_NEW = 1,
			CREATE_ALWAYS = 2,
			OPEN_EXISTING = 3,
			OPEN_ALWAYS = 4,
			TRUNCATE_EXISTING = 5
		}

		/// <summary>
		/// Mailslot waits forever for a message
		/// </summary>
		public const int MAILSLOT_WAIT_FOREVER = -1;

		/// <summary>
		/// There is no next message
		/// </summary>
		public const int MAILSLOT_NO_MESSAGE = -1;


		/// <summary>
		/// Represents a wrapper class for a mailslot handle.
		/// </summary>
		[SecurityCritical(SecurityCriticalScope.Everything),
		 HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true),
		 SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)
		]
		
		public sealed class SafeMailslotHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			private SafeMailslotHandle() : base(true)
			{
			}

			public SafeMailslotHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
			{
				base.SetHandle(preexistingHandle);
			}

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			private static extern bool CloseHandle(IntPtr handle);

			protected override bool ReleaseHandle()
			{
				try
				{
					return CloseHandle(base.handle);
				} catch
				{
					return false;
				}
			}
		}


		/// <summary>
		/// The SECURITY_ATTRIBUTES structure contains the security descriptor for
		/// an object and specifies whether the handle retrieved by specifying
		/// this structure is inheritable. This structure provides security
		/// settings for objects created by various functions, such as CreateFile,
		/// CreateNamedPipe, CreateProcess, RegCreateKeyEx, or RegSaveKeyEx.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public class SECURITY_ATTRIBUTES
		{
			public int nLength;
			public SafeLocalMemHandle lpSecurityDescriptor;
			public bool bInheritHandle;
		}


		/// <summary>
		/// Represents a wrapper class for a local memory pointer.
		/// </summary>
		[SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
		public sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			public SafeLocalMemHandle() : base(true)
			{
			}

			public SafeLocalMemHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
			{
				base.SetHandle(preexistingHandle);
			}

			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			private static extern IntPtr LocalFree(IntPtr hMem);

			protected override bool ReleaseHandle()
			{
				return (LocalFree(base.handle) == IntPtr.Zero);
			}
		}


		/// <summary>
		/// The class exposes Windows APIs to be used in this code sample.
		/// </summary>
		[SuppressUnmanagedCodeSecurity]
		public class MailSlot
		{
			/// <summary>
			/// Creates an instance of a mailslot and returns a handle for subsequent
			/// operations.
			/// </summary>
			/// <param name="mailslotName">Mailslot name</param>
			/// <param name="nMaxMessageSize">
			/// The maximum size of a single message
			/// </param>
			/// <param name="lReadTimeout">
			/// The time a read operation can wait for a message.
			/// </param>
			/// <param name="securityAttributes">Security attributes</param>
			/// <returns>
			/// If the function succeeds, the return value is a handle to the server
			/// end of a mailslot instance.
			/// </returns>
			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern SafeMailslotHandle CreateMailslot(string mailslotName,
			                                                       uint nMaxMessageSize,
			                                                       int lReadTimeout,
			                                                       SECURITY_ATTRIBUTES securityAttributes);


			/// <summary>
			/// Creates or opens a file, directory, physical disk, volume, console
			/// buffer, tape drive, communications resource, mailslot, or named pipe.
			/// </summary>
			/// <param name="fileName">
			/// The name of the file or device to be created or opened.
			/// </param>
			/// <param name="desiredAccess">
			/// The requested access to the file or device, which can be summarized
			/// as read, write, both or neither (zero).
			/// </param>
			/// <param name="shareMode">
			/// The requested sharing mode of the file or device, which can be read,
			/// write, both, delete, all of these, or none (refer to the following
			/// table).
			/// </param>
			/// <param name="securityAttributes">
			/// A SECURITY_ATTRIBUTES object that contains two separate but related
			/// data members: an optional security descriptor, and a Boolean value
			/// that determines whether the returned handle can be inherited by
			/// child processes.
			/// </param>
			/// <param name="creationDisposition">
			/// An action to take on a file or device that exists or does not exist.
			/// </param>
			/// <param name="flagsAndAttributes">
			/// The file or device attributes and flags.
			/// </param>
			/// <param name="hTemplateFile">Handle to a template file.</param>
			/// <returns>
			/// If the function succeeds, the return value is an open handle to the
			/// specified file, device, named pipe, or mail slot.
			/// If the function fails, the return value is an invalid handle.
			/// </returns>
			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern SafeMailslotHandle CreateFile(string fileName,
			                                                   FileDesiredAccess desiredAccess,
			                                                   FileShareMode shareMode,
			                                                   IntPtr securityAttributes,
			                                                   FileCreationDisposition creationDisposition,
			                                                   int flagsAndAttributes,
			                                                   IntPtr hTemplateFile);
			
			/// <summary>
			/// Retrieves information about the specified mailslot.
			/// </summary>
			/// <param name="hMailslot">A handle to a mailslot</param>
			/// <param name="lpMaxMessageSize">
			/// The maximum message size, in bytes, allowed for this mailslot.
			/// </param>
			/// <param name="lpNextSize">
			/// The size of the next message in bytes.
			/// </param>
			/// <param name="lpMessageCount">
			/// The total number of messages waiting to be read.
			/// </param>
			/// <param name="lpReadTimeout">
			/// The amount of time, in milliseconds, a read operation can wait for a
			/// message to be written to the mailslot before a time-out occurs.
			/// </param>
			/// <returns></returns>
			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool GetMailslotInfo(SafeMailslotHandle hMailslot,
			                                          IntPtr lpMaxMessageSize,
			                                          out int lpNextSize,
			                                          out int lpMessageCount,
			                                          IntPtr lpReadTimeout);


			/// <summary>
			/// Reads data from the specified file or input/output (I/O) device.
			/// </summary>
			/// <param name="handle">
			/// A handle to the device (for example, a file, file stream, physical
			/// disk, volume, console buffer, tape drive, socket, communications
			/// resource, mailslot, or pipe).
			/// </param>
			/// <param name="bytes">
			/// A buffer that receives the data read from a file or device.
			/// </param>
			/// <param name="numBytesToRead">
			/// The maximum number of bytes to be read.
			/// </param>
			/// <param name="numBytesRead">
			/// The number of bytes read when using a synchronous IO.
			/// </param>
			/// <param name="overlapped">
			/// A pointer to an OVERLAPPED structure if the file was opened with
			/// FILE_FLAG_OVERLAPPED.
			/// </param>
			/// <returns>
			/// If the function succeeds, the return value is true. If the function
			/// fails, or is completing asynchronously, the return value is false.
			/// </returns>
			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool ReadFile(SafeMailslotHandle handle,
			                                   byte[] bytes,
			                                   int numBytesToRead,
			                                   out int numBytesRead,
			                                   IntPtr overlapped);


			/// <summary>
			/// Writes data to the specified file or input/output (I/O) device.
			/// </summary>
			/// <param name="handle">
			/// A handle to the file or I/O device (for example, a file, file stream,
			/// physical disk, volume, console buffer, tape drive, socket,
			/// communications resource, mailslot, or pipe).
			/// </param>
			/// <param name="bytes">
			/// A buffer containing the data to be written to the file or device.
			/// </param>
			/// <param name="numBytesToWrite">
			/// The number of bytes to be written to the file or device.
			/// </param>
			/// <param name="numBytesWritten">
			/// The number of bytes written when using a synchronous IO.
			/// </param>
			/// <param name="overlapped">
			/// A pointer to an OVERLAPPED structure is required if the file was
			/// opened with FILE_FLAG_OVERLAPPED.
			/// </param>
			/// <returns>
			/// If the function succeeds, the return value is true. If the function
			/// fails, or is completing asynchronously, the return value is false.
			/// </returns>
			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool WriteFile(SafeMailslotHandle handle,
			                                    byte[] bytes,
			                                    int numBytesToWrite,
			                                    out int numBytesWritten,
			                                    IntPtr overlapped);
			
			/// <summary>
			/// The ConvertStringSecurityDescriptorToSecurityDescriptor function
			/// converts a string-format security descriptor into a valid,
			/// functional security descriptor.
			/// </summary>
			/// <param name="sddlSecurityDescriptor">
			/// A string containing the string-format security descriptor (SDDL)
			/// to convert.
			/// </param>
			/// <param name="sddlRevision">
			/// The revision level of the sddlSecurityDescriptor string.
			/// Currently this value must be 1.
			/// </param>
			/// <param name="pSecurityDescriptor">
			/// A pointer to a variable that receives a pointer to the converted
			/// security descriptor.
			/// </param>
			/// <param name="securityDescriptorSize">
			/// A pointer to a variable that receives the size, in bytes, of the
			/// converted security descriptor. This parameter can be IntPtr.Zero.
			/// </param>
			/// <returns>
			/// If the function succeeds, the return value is true.
			/// </returns>
			[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
				string sddlSecurityDescriptor,
				int sddlRevision,
				out SafeLocalMemHandle pSecurityDescriptor,
				IntPtr securityDescriptorSize);
			
			
			
			
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool CloseHandle(SafeMailslotHandle handle);
			
		}
		#endregion
	}
}

