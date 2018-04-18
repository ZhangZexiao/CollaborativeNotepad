using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace HFDMwithCSharpByHarryZhang
{
	public partial class Form_CollaborativeNotepad:
	Form
	{
		Lynx.PropertySets.HFDM hfdm;
		Lynx.PropertySets.Workspace workspace;
		const string KEYPRESS="KeyPress";
		const string KEYPRESSCOUNT="KeyPressCount";
		private string getKeyPressName(int i)
		{
			return KEYPRESS+i.ToString("D8");
		}
		private bool isEmptyBranchGuid(string newBranchGuid)
		{
			return string.IsNullOrWhiteSpace(newBranchGuid);
		}
		private string getBranchGuidFromUserInterface()
		{
			return comboBox_BranchGuid.Text;
		}
		private void setBranchGuidOnUserInterface(string newBranchGuid)
		{
			comboBox_BranchGuid.Text=newBranchGuid;
		}
		private void button_Checkout_Click(object sender,EventArgs e)
		{
			if((hfdm?.isConnected()).GetValueOrDefault(false)&&!(workspace?.isCheckedOut()).GetValueOrDefault(true))
			{
				if(isEmptyBranchGuid(getBranchGuidFromUserInterface()))
				{
					setBranchGuidOnUserInterface(createRepository(hfdm)[0].branchNode.getGuid());
					sessionLogger.Log("***created*** branch guid: "+getBranchGuidFromUserInterface());
				}
				if(!string.IsNullOrEmpty(currentPropertySetsServerUrl.inspectorUrl))
				{
					var inspectorUrl=currentPropertySetsServerUrl.inspectorUrl+getBranchGuidFromUserInterface();
					System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(inspectorUrl));
					cefBrowser.Load(inspectorUrl);
					sessionLogger.Log("inspector url: "+inspectorUrl);
				}
				checkoutWorkspace(workspace,getBranchGuidFromUserInterface());
				joinBranch(hfdm,workspace,getBranchGuidFromUserInterface());
			}
		}
		private void richTextBox_Notepad_KeyPress(object sender,KeyPressEventArgs e)
		{
			//get key press count
			Lynx.PropertySets.Int32Property keyPressCountProperty=workspace.resolvePathInt32Property(KEYPRESSCOUNT);
			if(null==keyPressCountProperty)
			{
				keyPressCountProperty=LynxPropertySetsCSharp.GetPropertyFactory().createInt32Property();
				keyPressCountProperty.setValue(0);
				workspace.insert(KEYPRESSCOUNT,keyPressCountProperty);
			}
			var count=keyPressCountProperty.getValue();
			sessionLogger.Log("key press count: "+count);
			//add a string property
			var stringProperty=LynxPropertySetsCSharp.GetPropertyFactory().createStringProperty();
			stringProperty.setValue(System.Text.Encoding.ASCII.GetString(BitConverter.GetBytes(e.KeyChar)));
			workspace.insert(getKeyPressName(count),stringProperty);
			//update key press count
			count++;
			keyPressCountProperty.setValue(count);
			//commit
			if(workspace.hasPendingChanges())
			{
				commitPendingChanges(workspace);
			}
			prettyPrintWorkspace(workspace);
		}
		private void prettyPrintWorkspace(Lynx.PropertySets.Workspace workspace)
		{
			sessionLogger.Log("workspace active branch guid: "+workspace.getActiveBranch().getGuid());
			sessionLogger.Log("workspace active branch head guid: "+workspace.getActiveBranch().getHEAD().getGuid());
			System.IO.MemoryStream memoryStream=new System.IO.MemoryStream();
			workspace.prettyPrint(memoryStream);
			sessionLogger.Log("workspace root pretty print: "+System.Text.Encoding.ASCII.GetString(memoryStream.ToArray()));
		}
		private bool joinBranch(Lynx.PropertySets.HFDM hfdm,Lynx.PropertySets.Workspace workspace,string branchGuid)
		{
			bool result=true;
			sessionLogger.Log("join branch: "+branchGuid);
			using(var eventWaitHandle=new EventWaitHandle(false,EventResetMode.ManualReset))
			{
				hfdm.join(branchGuid,(error,repository)=>{if(error!=null){System.Windows.Forms.MessageBox.Show(error.what());sessionLogger.Log("error: "+error.what());result=false;}else{checkoutWorkspace(workspace,branchGuid);}eventWaitHandle.Set();});
				eventWaitHandle.WaitOne();
			}
			return result;
		}
		private LynxRepositoryInfoVector createRepository(Lynx.PropertySets.HFDM hfdm)
		{
			LynxRepositoryInfoVector repositoryInfoVector=null;
			sessionLogger.Log("create repository.");
			using(var eventWaitHandle=new EventWaitHandle(false,EventResetMode.ManualReset))
			{
				hfdm.createRepository(false,(error,lynxRepositoryInfoVector)=>{if(error!=null){System.Windows.Forms.MessageBox.Show(error.what());sessionLogger.Log("error: "+error.what());}else if(lynxRepositoryInfoVector.Count<1){const string errorMessage="Create repository failed.";System.Windows.Forms.MessageBox.Show(errorMessage);sessionLogger.Log("error: "+errorMessage);}else{repositoryInfoVector=lynxRepositoryInfoVector;}eventWaitHandle.Set();});
				eventWaitHandle.WaitOne();
			}
			return repositoryInfoVector;
		}
		private void synchronizeWorkspace(Lynx.PropertySets.Workspace workspace)
		{
			if(!workspace.isSynchronized())
			{
				sessionLogger.Log("synchronize workspace.");
				using(var eventWaitHandle=new EventWaitHandle(false,EventResetMode.ManualReset))
				{
					workspace.synchronize((error,commitNodeVector)=>{if(error!=null){System.Windows.Forms.MessageBox.Show(error.what());sessionLogger.Log("error: "+error.what());}else{sessionLogger.Log("workspace synchronized: "+commitNodeVector.Count);}eventWaitHandle.Set();});
					eventWaitHandle.WaitOne();
				}
			}
		}
		private void commitPendingChanges(Lynx.PropertySets.Workspace workspace)
		{
			sessionLogger.Log("commit pending changes.");
			using(var eventWaitHandle=new EventWaitHandle(false,EventResetMode.ManualReset))
			{
				workspace.commit(DateTime.Now.ToString(),(error,commitNode)=>{if(error!=null){System.Windows.Forms.MessageBox.Show(error.what());sessionLogger.Log("error: "+error.what());}eventWaitHandle.Set();});
				eventWaitHandle.WaitOne();
			}
		}
		private bool checkoutWorkspace(Lynx.PropertySets.Workspace workspace,string branchGuid)
		{
			sessionLogger.Log("checkout workspace guid: "+branchGuid);
			if(!workspace.checkout(branchGuid))
			{
				const string errorMessage="Checkout workspace failed.";
				System.Windows.Forms.MessageBox.Show(errorMessage);
				sessionLogger.Log("error: "+errorMessage);
				return false;
			}
			//BUG: refreshRichTextBox called three times!!!
			//workspace.setAutoUpdate(true,(error)=>{if(error!=null){System.Windows.Forms.MessageBox.Show(error.what());sessionLogger.Log("error: "+error.what());}refreshRichTextBox();});
			return true;
		}
		public static class PSetHelper
		{
			/**
             * Connect to the backend
             *
             * @param hfdm           - Repository manager
             * @param serverURL      - Backend (PSS) url
             * @param getBearerToken - Handler should return valid OAuth2 token
             */
			public static bool connect(Lynx.PropertySets.HFDM hfdm,string serverURL)
			{
				return connect(hfdm,serverURL,null);
			}
			public static bool connect(Lynx.PropertySets.HFDM hfdm,string serverURL,Func<string>getBearerToken)
			{
				bool result=false;
				using(var barrier=new EventWaitHandle(false,EventResetMode.ManualReset))
				{
					hfdm.connect(serverURL,(error)=>{if(error!=null){Console.Error.WriteLine(error.what());System.Windows.Forms.MessageBox.Show(error.what());}else{result=true;}barrier.Set();},getBearerToken);
					barrier.WaitOne();
				}
				return result;
			}
			/**
             * Create local repository.
             *
             * @param hfdm    - Repository manager
             */
			public static LynxRepositoryInfoVector createLocalRepository(Lynx.PropertySets.HFDM hfdm)
			{
				LynxRepositoryInfoVector result=null;
				using(var barrier=new EventWaitHandle(false,EventResetMode.ManualReset))
				{
					hfdm.createRepository(true,(error,repos)=>{if(error!=null){Console.Error.WriteLine("Cannot create repository: "+error.what());}else if(repos.Count<1){Console.Error.WriteLine("No repository info found");}else{var repository=repos[0].repository;Console.WriteLine("Successfully created local repository with GUID: "+repository.getGuid());result=repos;}barrier.Set();});
					barrier.WaitOne();
				}
				return result;
			}
			/**
             * Commits the PropertySet changes
             *
             * @param workspace     - Workspace object encapsulates a HFDM to proxy function calls.
             * @param commitMessage - Message attached as metadata describing the commit
             *
             * @return future indicating commit is finished
             */
			public static bool commitChanges(Lynx.PropertySets.Workspace workspace,string commitMessage)
			{
				bool result=false;
				using(var barrier=new EventWaitHandle(false,EventResetMode.ManualReset))
				{
					workspace.commit(commitMessage,(error,commitNode)=>{if(error!=null){Console.WriteLine("Failed to commit: "+error.what());}else{Console.WriteLine("Changes commited: commit "+commitNode.getGuid());result=true;}barrier.Set();});
					barrier.WaitOne();
				}
				return result;
			}
			/**
             * Create local or remote repository.
             *
             * @param hfdm    - Repository manager
             * @param isLocal - If true then create local repository, otherwise remote
             */
			public static LynxRepositoryInfoVector createRepository(Lynx.PropertySets.HFDM hfdm,bool isLocal)
			{
				LynxRepositoryInfoVector result=null;
				using(var barrier=new EventWaitHandle(false,EventResetMode.ManualReset))
				{
					hfdm.createRepository(isLocal,(error,repos)=>{if(error!=null){Console.WriteLine("Cannot create repository: "+error.what());}else if(repos.Count<1){Console.Error.WriteLine("No repository info found");}else{result=repos;Lynx.PropertySets.Repository repository=repos[0].repository;Console.WriteLine("Successfully created "+(repository.isLocal()?"local":"remote")+" repository with GUID: "+repository.getGuid());}barrier.Set();});
					barrier.WaitOne();
				}
				return result;
			}
			/**
             * Checks out the branch corresponding to the branchGuid
             *
             * @param error       - Indicates of an error happened in the code calling this function
             * @param branchGuid  - branch identifier
             * @param workspace   - Workspace object encapsulates a HFDM to proxy function calls.
             */
			public static void checkoutAfterJoin(exception error,string branchGuid,Lynx.PropertySets.Workspace workspace,SampleApplication application)
			{
				if(error!=null)
				{
					Console.WriteLine(" Could not join room: "+error.what());
					Console.Error.WriteLine(error.what());
					return;
				}
				Console.WriteLine("Successfully joined room: "+branchGuid);
				// get the head version of the given branch
				if(!workspace.checkout(branchGuid))
				{
					Console.Error.WriteLine("Checkout branch: "+branchGuid+" failed");
					return;
				}
				// Register event to handle PropertySet commits
				workspace.setAutoUpdate(true,application.autoUpdateHandler);
				application.onJoin();
			}
			/**
             * Joins a local room
             *
             * @param branchGuid  - Branch identifier
             * @param workspace   - Workspace object encapsulates a HFDM to proxy function calls
             * @param application - Sample application
             */
			public static void joinLocalRoom(string branchGuid,Lynx.PropertySets.Workspace workspace,SampleApplication application)
			{
				checkoutAfterJoin(null,branchGuid,workspace,application);
			}
			/**
             * Join the room corresponding to the given branch on the property sets server
             *
             * @param hfdm       - High Frequency Data Management instance
             * @param workspace  - Workspace object encapsulates a HFDM to proxy function calls.
             * @param branchGuid - Branch identifier
             *
             * @return future indicating whether room joining callback has been executed
             */
			public static bool joinRoom(Lynx.PropertySets.HFDM hfdm,Lynx.PropertySets.Workspace workspace,string branchGuid,SampleApplication application)
			{
				bool result=false;
				using(var barrier=new EventWaitHandle(false,EventResetMode.ManualReset))
				{
					hfdm.join(branchGuid,(error,repository)=>{checkoutAfterJoin(error,branchGuid,workspace,application);if(error==null){result=true;}barrier.Set();});
					barrier.WaitOne();
				}
				return result;
			}
			/**
             * Joins the branch session described in the repositoryInfo and start interaction (add/delete/print) with the
             * property set
             *
             * @param hfdm        - High Frequency Data Management instance
             * @param workspace   - Workspace object encapsulates a HFDM to proxy function calls
             * @param branchGuid  - Branch identifier
             * @param application - Sample application
             */
			public static void joinBranch(Lynx.PropertySets.HFDM hfdm,Lynx.PropertySets.Workspace workspace,string branchGuid,SampleApplication application)
			{
				// Make the point template known to the property manager
				application.registerTemplates();
				application.registerCommands();
				// join the room of the repository
				if(hfdm.isConnected())
				{
					joinRoom(hfdm,workspace,branchGuid,application);
				}
				else
				{
					joinLocalRoom(branchGuid,workspace,application);
				}
				// application loop
				while(application.tick())
				{
				}
			}
		}
		class AppCommand
		{
			public string description;
			public Action callback;
		};
		class AcceleratorOrSeparator
		{
			public AcceleratorOrSeparator(string text)
			{
				this.text=text;
				this.isSeparator=false;
			}
			public AcceleratorOrSeparator(string text,bool isSeparator)
			{
				this.text=text;
				this.isSeparator=isSeparator;
			}
			public string text;
			public bool isSeparator;
		};
		public class SampleApplication
		{
			public void setHfdm(Lynx.PropertySets.HFDM hfdm)
			{
				m_hfdm=hfdm;
			}
			/**
             * Stores the workspace to operate on for later use
             */
			public void setWorkspace(Lynx.PropertySets.Workspace workspace)
			{
				m_workspace=workspace;
			}
			/**
             * Toggle auto commit
             */
			public void setAutoCommit(bool isAutoCorrect)
			{
				m_isAutoCommit=isAutoCorrect;
			}
			/**
             * Will be called for every OnBranchHeadMoved event
             */
			public virtual void autoUpdateHandler(exception error)
			{
				if(error!=null)
				{
					Console.WriteLine("####################");
					Console.WriteLine("AutoUpdate error: "+error.what());
					Console.WriteLine("####################");
					Console.WriteLine();
					return;
				}
				updateCurrentState();
			}
			/**
             * Update the current state of branch
             */
			public virtual void updateCurrentState()
			{
			}
			/**
             * @return Description of the sample application
             */
			public virtual string getDescription()
			{
				return string.Empty;
			}
			/**
             * @return Commit message of the sample application
             */
			public virtual string getCommitMessage()
			{
				return"Default commit message";
			}
			/**
             * Can be used to register needed templates
             */
			public virtual void registerTemplates()
			{
			}
			/**
             * Can be used to register needed commands
             */
			public virtual void registerCommands()
			{
			}
			/**
             * Register application command
             *
             * @param accelerator - CLI input to match to select this command
             * @param description - Text to display
             * @param callback    - Callback to invoke if selected
             */
			public void addCommand(string accelerator,string description,Action callback)
			{
				if(m_commands.Count==0|| !m_commands.ContainsKey(accelerator))
				{
					var command=new AppCommand
					{
						description=description,callback=callback
					};
					m_commands.Add(accelerator,command);
					m_orderedCommands.Add(new AcceleratorOrSeparator(accelerator));
				}
				else
				{
					Console.WriteLine("Accelerator key already registered: "+accelerator);
				}
			}
			/**
             * Register separator between application commands
             *
             * @param separator - Text to display
             */
			public void addSeparator(string separator)
			{
				m_orderedCommands.Add(new AcceleratorOrSeparator(separator,true));
			}
			/**
             * Print all available commands
             */
			public void printCommands()
			{
				Console.WriteLine("####################");
				Console.WriteLine("#Available commands#");
				Console.WriteLine("####################");
				Console.WriteLine();
				foreach(var acceleratorOrSeparator in m_orderedCommands)
				{
					if(acceleratorOrSeparator.isSeparator)
					{
						Console.WriteLine(acceleratorOrSeparator.text);
					}
					else
					{
						Console.WriteLine(acceleratorOrSeparator.text+": "+m_commands[acceleratorOrSeparator.text].description);
					}
				}
				Console.WriteLine("exit: Exit the application");
			}
			/**
             * Execute selected command
             *
             * @param accelerator - CLI input to match to select command
             */
			public void executeCommand(string accelerator)
			{
				if(m_commands.ContainsKey(accelerator))
				{
					m_commands[accelerator].callback();
				}
				else
				{
					Console.WriteLine("Command not found.");
				}
			}
			/**
             * Called just before the first tick
             */
			public virtual void initApplication()
			{
			}
			/**
             * Will be called in a loop until it returns false
             */
			public virtual bool tick()
			{
				ApplicationHelper.getLineAsync(out m_input,m_workspace);
				m_workspace.waitUntilNextEvent();
				if(m_input!="not empty")
				{
					if(m_input=="exit")
					{
						return false;
					}
					// delay internal eventing till all changes have been applied
					m_workspace.pushModifiedEventScope();
					executeCommand(m_input);
					// reactivate internal eventing and process the changes
					m_workspace.popModifiedEventScope();
					if(m_workspace.hasPendingChanges()&&m_isAutoCommit)
					{
						PSetHelper.commitChanges(m_workspace,getCommitMessage());
					}
					updateCurrentState();
					printCommands();
				}
				return true;
			}
			/**
             * Will be called when we have joined a branch
             */
			public virtual void onJoin()
			{
				initApplication();
				registerCommonCommands();
				updateCurrentState();
				printCommands();
			}
			/**
             * Prompt for a property path.
             *
             * @param prompt      - Optional prompt to display. If empty, a default prompt will be used.
             * @param outputGuid  - GUID entered by user (only modified if true is returned).
             *
             * @return true if the user entered a valid GUID, false otherwise.
             */
			public bool promptForGuid(string prompt,out string outputGuid)
			{
				outputGuid=null;
				string promptString;
				if(string.IsNullOrWhiteSpace(prompt))
				{
					promptString="Enter GUID (return to cancel): ";
				}
				else
				{
					promptString=prompt;
				}
				string guidString=null;
				while(string.IsNullOrWhiteSpace(guidString))
				{
					Console.WriteLine();
					Console.Write(promptString);
					string input=null;
					ApplicationHelper.getLineAsync(out input,m_workspace);
					m_workspace.waitUntilNextEvent();
					if(string.IsNullOrWhiteSpace(input))
					{
						break;
					}
					guidString=input;
				}
				if(string.IsNullOrWhiteSpace(guidString))
				{
					return false;
				}
				outputGuid=guidString;
				return true;
			}
			void registerCommonCommands()
			{
				addSeparator("-----");
				addCommand("pw","Print the workspace",()=>ApplicationHelper.printWorkspace(m_workspace));
				addCommand("sar","Show active repository",()=>ApplicationHelper.showActiveRepository(m_workspace));
				addCommand("sab","Show active branch",()=>ApplicationHelper.showActiveBranch(m_workspace));
				addCommand("sac","Show active commit",()=>ApplicationHelper.showActiveCommit(m_workspace));
				addSeparator("-----");
			}
			protected Lynx.PropertySets.HFDM m_hfdm=null;
			protected Lynx.PropertySets.Workspace m_workspace=null;
			protected string m_input=null;
			// Map from accelerator to registered command
			Dictionary<string,AppCommand>m_commands=new Dictionary<string,AppCommand>();
			// Maintain insertion order of all registered commands and separators
			List<AcceleratorOrSeparator>m_orderedCommands=new List<AcceleratorOrSeparator>();
			bool m_isAutoCommit=true;
		}
		public static class ApplicationHelper
		{
			/**
             * Does a getline in a different thread and signals the workspace to getline finished
             *
             * @param line      - String to write the line into
             * @param workspace - Workspace object to dispatch the signal
             */
			public static void getLineAsync(out string line,Lynx.PropertySets.Workspace workspace)
			{
				line="not empty";
				string read=string.Empty;
				using(var barrier=new EventWaitHandle(false,EventResetMode.ManualReset))
				{
					workspace.queueEvent(()=>{read=Console.ReadLine();barrier.Set();});
					barrier.WaitOne();
					line=read;
				}
			}
			/*
             *   ###################################
             *   #           Templates             #
             *   ###################################
             */
			/**
             * Registers the template for a simple point
             */
			public static void registerPointTemplate()
			{
				string pointStr= @"
              {
                    ""inherits"": [""NamedProperty""],
                ""typeid"": ""autodesk.test:PointEntityID-1.0.0"",
                ""properties"": [
                  {
                    ""id"": ""position"",
                    ""properties"": [
                      {""id"":""x"",""typeid"":""Float32""},
                      {""id"":""y"",""typeid"":""Float32""}
                    ]
                  },
                  {
                    ""id"":""color"",
                    ""properties"": [
                      {""id"":""r"",""typeid"":""Uint32""},
                      {""id"":""g"",""typeid"":""Uint32""},
                      {""id"":""b"",""typeid"":""Uint32""}
                    ]
                  }
                ]
              }""
              ";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(pointStr);
			}
			/**
             * Registers the template for an array of points
             *
             * include registerPointTemplate()
             */
			public static void registerPointArrayTemplate()
			{
				registerPointTemplate();
				string pointArrayStr= @"
              {
                ""typeid"": ""autodesk.test:PointEntityIDArray-1.0.0"",
                ""properties"": [
                  { ""id"": ""points"", ""typeid"":""autodesk.test:PointEntityID-1.0.0"", ""context"": ""array"" }
                ]
              }""
              ";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(pointArrayStr);
			}
			/**
             * Registers the template for an array of primitive points
             */
			public static void registerPrimitivePointArrayTemplate()
			{
				string pointStr= @"
              {
                ""inherits"": [""NamedProperty""],
                ""typeid"": ""autodesk.test:PointPrimitiveArrayEntityID-1.0.0"",
                ""properties"": [
                  { ""id"": ""position"", ""typeid"": ""Float32"", ""context"": ""array"", ""length"": 2 },
                  { ""id"": ""color"", ""typeid"": ""Uint32"", ""context"": ""array"", ""length"": 3 }
                ]
              }""
              ";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(pointStr);
				string pointArrayStr= @"
              {
                ""typeid"": ""autodesk.test:PointPrimitiveArrayEntityIDArray-1.0.0"",
                ""properties"": [
                  { ""id"": ""points"", ""typeid"":""autodesk.test:PointPrimitiveArrayEntityID-1.0.0"", ""context"": ""array"" }
                ]
              }""
              ";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(pointArrayStr);
			}
			/**
             * Registers the template for a set of points
             *
             * include registerPointTemplate()
             */
			public static void registerPointSetTemplate()
			{
				registerPointTemplate();
				string pointSetStr= @"
              {
                ""typeid"": ""autodesk.test:PointEntityIDSet-1.0.0"",
                ""properties"": [
                  { ""id"": ""points"", ""typeid"":""autodesk.test:PointEntityID-1.0.0"", ""context"": ""set"" }
                ]
              }""
              ";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(pointSetStr);
			}
			/**
             * Registers the template for a map of points
             *
             * include registerPointTemplate()
             */
			public static void registerPointMapTemplate()
			{
				registerPointTemplate();
				string pointMapStr= @"
              {
                ""typeid"": ""autodesk.test:PointEntityIDSet-1.0.0"",
                ""properties"": [
                  { ""id"": ""points"", ""typeid"":""autodesk.test:PointEntityID-1.0.0"", ""context"": ""map"" }
                ]
              }""
              ";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(pointMapStr);
			}
			/**
             * Registers the template for a map of primitive points
             */
			public static void registerPrimitivePointMapTemplate()
			{
				string pointMapPrimitivesStr= @"
              {
                ""typeid"": ""autodesk.test:PointEntityIDMap-1.0.0"",
                ""properties"": [
                  { ""id"": ""points"",
                    ""properties"": [
                      {
                        ""id"": ""position"",
                        ""properties"": [
                          { ""id"" : ""x"", ""typeid"" : ""Float32"", ""context"" : ""map"" },
                          { ""id"" : ""y"", ""typeid"" : ""Float32"", ""context"" : ""map"" }
                        ]
                      },
                      {
                        ""id"":""color"",
                        ""properties"": [
                          { ""id"" : ""r"", ""typeid"":""Uint64"", ""context"" : ""map"" },
                          { ""id"" : ""g"", ""typeid"":""Uint32"", ""context"" : ""map"" },
                          { ""id"" : ""b"", ""typeid"":""Uint32"", ""context"" : ""map"" }
                        ]
                      }
                    ]
                  }
                ]
              }""
              ";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(pointMapPrimitivesStr);
			}
			/**
             * Registers the template for a switch using an enum to store its state
             */
			public static void registerSwitchTemplate()
			{
				string switchStr= @"
              {
                ""typeid"": ""autodesk.test:Switch-1.0.0"",
                ""properties"": [
                  { ""id"": ""location"", ""typeid"": ""String"" },
                  { ""id"": ""state"", ""typeid"": ""Enum"", ""properties"": [
                      { ""id"": ""off"" , ""value"": 0, ""annotation"": { ""description"": ""switch is turned off""  } },
                      { ""id"": ""on"", ""value"": 1, ""annotation"": { ""description"": ""switch is turned on"" } },
                      { ""id"": ""not responding"", ""value"": -1, ""annotation"": { ""description"": ""switch is not responding"" } }
                    ]
                  }
                ]
              }
              ";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(switchStr);
			}
			/**
             * Registers the template for a reference
             */
			public static void registerReferenceTemplate()
			{
				string refPropStr= @"
              {
                ""typeid"": ""autodesk.test:ReferenceID-1.0.0"",
                ""properties"": [{ ""id"": ""ref"", ""typeid"": ""Reference"" }]
              }""
              ";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(refPropStr);
			}
			/**
             * Registers the template for a custom string
             */
			public static void registerCustomStringTemplate()
			{
				string myStringProperty= @"
              {
                ""typeid"": ""autodesk.samples:string_sample.string-1.0.0"",
                ""inherits"": [""NamedProperty""],
                ""properties"": [
                  { ""id"": ""string"", ""typeid"": ""String"" }
                ]
              }";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(myStringProperty);
				string myCustomProperty= @"
              {
                ""typeid"": ""autodesk.samples:string_sample.customProperty-1.0.0"",
                ""inherits"": [""NamedProperty""],
                ""properties"": [
                  { ""id"": ""arrayOfCustom"", ""typeid"": ""autodesk.samples:string_sample.string-1.0.0"", ""context"": ""array"" },
                  { ""id"": ""mapOfCustom"", ""typeid"": ""autodesk.samples:string_sample.string-1.0.0"", ""context"": ""map"" },
                  { ""id"": ""arrayOfString"", ""typeid"": ""String"", ""context"": ""array"" },
                  { ""id"": ""mapOfString"", ""typeid"": ""String"", ""context"": ""map"" }
                ]
              }";
				LynxPropertySetsCSharp.GetPropertyFactory().registerTemplateFromJSON(myCustomProperty);
			}
			/**
             * Display information about the given commit.
             *
             * @param commitNode - commit to display.
             */
			public static void showCommit(Lynx.PropertySets.CommitNode commitNode)
			{
				Console.WriteLine();
				if(commitNode!=null)
				{
					string commitGuid=commitNode.getGuid();
					Console.WriteLine("Current commit: "+commitGuid);
					Console.Write("Parents: ");
					bool bFirst=true;
					foreach(var parent in commitNode.getParents())
					{
						if(bFirst)
						{
							bFirst=false;
						}
						else
						{
							Console.Write(", ");
						}
						Console.Write(parent.getGuid());
					}
					Console.WriteLine();
				}
				else
				{
					Console.WriteLine("No current commit.");
				}
				Console.WriteLine();
			}
			/**
             * Display the current checked out commit of the given workspace.
             *
             * @param workspace - Workspace to query.
             */
			public static void showActiveCommit(Lynx.PropertySets.Workspace workspace)
			{
				if(workspace!=null)
				{
					showCommit(workspace.getActiveCommit());
				}
				else
				{
					Console.WriteLine("No current workspace.");
				}
				Console.WriteLine();
			}
			/**
             * Display information about the given branch.
             *
             * @param branchNode - branch to display.
             */
			public static void showBranch(Lynx.PropertySets.BranchNode branchNode)
			{
				Console.WriteLine();
				if(branchNode!=null)
				{
					string branchGuid=branchNode.getGuid();
					string branchName=branchNode.getName();
					if(!string.IsNullOrWhiteSpace(branchName)&&branchName!=branchGuid)
					{
						Console.WriteLine("Current branch: "+branchName+" ("+branchGuid+")");
					}
					else
					{
						Console.WriteLine("Current branch: "+branchGuid);
					}
					Console.WriteLine("URN: "+branchNode.getURN());
				}
				else
				{
					Console.WriteLine("No current branch.");
				}
				Console.WriteLine();
			}
			/**
             * Display the current checked out branch of the given workspace.
             *
             * @param workspace - Workspace to query.
             */
			public static void showActiveBranch(Lynx.PropertySets.Workspace workspace)
			{
				if(workspace!=null)
				{
					showBranch(workspace.getActiveBranch());
				}
				else
				{
					Console.WriteLine("No current workspace.");
				}
				Console.WriteLine();
			}
			/**
             * Display information about the given repository.
             *
             * @param repository - repository to display.
             */
			public static void showRepository(Lynx.PropertySets.Repository repository)
			{
				Console.WriteLine();
				if(repository!=null)
				{
					string repositoryGuid=repository.getGuid();
					Console.WriteLine("Current repository: "+repository.getGuid()+" ("+(repository.isLocal()?"local":"remote")+")");
					Console.WriteLine("URN: "+repository.getURN());
				}
				else
				{
					Console.WriteLine("No current repository.");
				}
				Console.WriteLine();
			}
			/**
             * Display the current checked out repository of the given workspace.
             *
             * @param workspace - Workspace to query.
             */
			public static void showActiveRepository(Lynx.PropertySets.Workspace workspace)
			{
				if(workspace!=null)
				{
					showRepository(workspace.getActiveRepository());
				}
				else
				{
					Console.WriteLine("No current workspace.");
				}
				Console.WriteLine();
			}
			/**
             * Pretty print the workspace.
             *
             * @param workspace - Workspace to query.
             */
			public static void printWorkspace(Lynx.PropertySets.Workspace workspace)
			{
				if(workspace!=null)
				{
					var root=workspace.getRoot();
					Console.WriteLine();
					if(root!=null)
					{
						root.prettyPrint();
						Console.WriteLine();
					}
					else
					{
						Console.WriteLine("No current root.");
					}
				}
				else
				{
					Console.WriteLine("No current workspace.");
				}
				Console.WriteLine();
			}
		}
	}
}
