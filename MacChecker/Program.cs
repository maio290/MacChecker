/*
 * Created by SharpDevelop.
 * User: Mario.Hoffmann
 * Date: 05.12.2018
 * Time: 15:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
namespace MacChecker
{
	class Program
	{
		public static void Main(string[] args)
		{
			//Found here: https://code.wireshark.org/review/gitweb?p=wireshark.git;a=blob_plain;f=manuf
			string[] lines = System.IO.File.ReadAllLines("macList.txt");
			var macLookupTable = new Dictionary<string,string>();

			foreach(var line in lines)
			{
				// skip empty lines or comments
				if(line.Length == 0 || line[0].Equals('#'))
				{continue;}
				
				// split lines by tabstop
				string[] split = line.Split('\t');
				string key = split[0];
				string value = split[1];
				if(split.Length > 2)
				{
					value = split[2];
				}
				macLookupTable.Add(key,value);
			}
			
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();

            PowerShell ps = PowerShell.Create();
            ps.Runspace = runspace;
            ps.AddCommand("get-wmiobject").AddArgument("win32_networkadapterconfiguration").AddCommand("select").AddArgument("macaddress");
            
             foreach(PSObject result in ps.Invoke())
            {
             	if(result.Members["macaddress"].Value != null)
             	{
             		string macAddress = result.Members["macaddress"].Value.ToString().Substring(0,8);
             		string macVendor;
             		if(macLookupTable.TryGetValue(macAddress,out macVendor))
             		{
             			Console.WriteLine(result.Members["macaddress"].Value + " was manufactured by " + macVendor);
             		}
             		else
             		{
             			Console.WriteLine("No match found for "+result.Members["macaddress"].Value+" possibly virtual device!");
             		}
             	}
             		
            }           
            
            Console.ReadKey();
		}
	}
}