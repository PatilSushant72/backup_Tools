using System;
using System.IO;
using System.Data;
using System.Globalization;
using System.Data.OleDb;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Plc;
using Tecnomatix.Engineering.DataTypes;

using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Collections;

public class ExampleFindOrCreateLogicResource
{
    const string LOGIC_RESOURCE_NAME = "";

    //const string LOGIC_RESOURCE_NAME = logicResourceName;
    /// <summary>
    /// Entry method with console output
    /// </summary>
    /// <param name="output">Output string writer</param>
    public static void MainWithOutput(ref StringWriter output)
    {
        StringWriter output1 = new StringWriter();

        //OpenFileDialog openFileDialog = new OpenFileDialog();
        //openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
        //openFileDialog.Title = "Select a CSV File";
        string filePath = @"C:\Users\b6b8b2c7\Desktop\test.csv";


        //if (openFileDialog.ShowDialog() == DialogResult.OK)
        //{
        //   filePath = openFileDialog.FileName;
        //MessageBox.Show("Selected CSV file: " + filePath, "CSV File Selected");
        //}

        if (filePath == "")
        {
            //output.WriteLine("Select CSV File");
            return;
        }

        var entryList = new List<string[]>();
        var exitList = new List<string[]>();
        var paramList = new List<string[]>();
        var constantList = new List<string[]>();
        var conveyoractionList = new List<string[]>();
        var movposList = new List<string[]>();
        var movjointList = new List<string[]>();
        var gripactionList = new List<string[]>();
        var releaseactionList = new List<string[]>();
        var jumpjointactionList = new List<string[]>();
        var atposeparameterList = new List<string[]>();
        var jointdistanceparameterList = new List<string[]>();
        var linearunitsList = new List<string[]>();
        var jointVelocityControlled = new List<string[]>();
        var movetojointVal = new List<string[]>();
        var jointaccelerationControlledAction = new List<string[]>();
        string resourceName = "";
        //var lines = File.ReadAllLines(filePath);
        try
        {
            string[] lines = File.ReadAllLines(filePath);

            resourceName = lines[0].Split(';')[0];

            output.WriteLine(resourceName);

            string currentSection = "";
            foreach (var l in lines)
            {
                output.WriteLine(l);
            }
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Detect section headers
                if (line.StartsWith("Entry Name"))
                {
                    currentSection = "Entry";
                    continue;
                }
                else if (line.StartsWith("Exit Name"))
                {
                    currentSection = "Exit";
                    continue;
                }
                else if (line.StartsWith("Parameter"))
                {
                    currentSection = "Param";
                    continue;
                }
                else if (line.StartsWith("Constant"))
                {
                    currentSection = "Const";
                    continue;
                }
                else if (line.StartsWith("Conveyor Action"))
                {
                    currentSection = "ConvAct";
                    continue;
                }
                else if (line.StartsWith("Move To Pose Action"))
                {
                    currentSection = "movpos";
                    continue;
                }
                else if (line.StartsWith("Move Joint Action"))
                {
                    currentSection = "movjoint";
                    continue;
                }
                else if (line.StartsWith("Joint Velocity Controlled Action"))
                {
                    currentSection = "JointVelocityControlled";
                    continue;
                }

                else if (line.StartsWith("Grip Action"))
                {
                    currentSection = "gripaction";
                    continue;
                }
                else if (line.StartsWith("Release Action"))
                {
                    currentSection = "releaseaction";
                    continue;
                }
                else if (line.StartsWith("Jump Joint Action"))
                {
                    currentSection = "jumpjointaction";
                    continue;
                }
                else if (line.StartsWith("At Pose Parameter"))
                {
                    currentSection = "atposeparameter";
                    continue;
                }
                else if (line.StartsWith("Joint Distance Parameter"))
                {
                    currentSection = "jointdistanceparameter";
                    continue;
                }
                else if (line.StartsWith("Linear Units"))
                {
                    currentSection = "linearunits";
                    continue;
                }
                else if (line.StartsWith("Move Joint To Value"))
                {
                    currentSection = "MoveJointToVal";
                    continue;
                }
                else if (line.StartsWith("Joint Acceleration Controlled Action"))
                {
                    currentSection = "JointAccelerationControlledAction";
                    continue;
                }
                // Split row into columns
                var row = line.Split(';');

                // Add to respective list
                switch (currentSection)
                {
                    case "Entry":
                        entryList.Add(row);
                        break;
                    case "Exit":
                        exitList.Add(row);
                        break;
                    case "Param":
                        paramList.Add(row);
                        break;
                    case "Const":
                        constantList.Add(row);
                        break;
                    case "ConvAct":
                        conveyoractionList.Add(row);
                        break;
                    case "movpos":
                        movposList.Add(row);
                        break;
                    case "movjoint":
                        movjointList.Add(row);
                        break;
                    case "gripaction":
                        gripactionList.Add(row);
                        break;
                    case "releaseaction":
                        releaseactionList.Add(row);
                        break;
                    case "jumpjointaction":
                        jumpjointactionList.Add(row);
                        break;
                    case "atposeparameter":
                        atposeparameterList.Add(row);
                        break;
                    case "jointdistanceparameter":
                        jointdistanceparameterList.Add(row);
                        break;
                    case "linearunits":
                        linearunitsList.Add(row);
                        break;
                    case "JointVelocityControlled":
                        linearunitsList.Add(row);
                        break;
                    case "MoveJointToVal":
                        movetojointVal.Add(row);
                        break;
                    case "JointAccelerationControlledAction":
                        jointaccelerationControlledAction.Add(row);
                        break;
                }
            }
        }
        catch (Exception exc)
        {
            output.WriteLine(exc.Message);
            return;
        }

        if (constantList.Count > 0)
        {
            output.WriteLine("Yes");
        }


        TxTypeFilter robotFilter = new TxTypeFilter(typeof(ITxDevice));
        TxObjectList robots = TxApplication.ActiveDocument.PhysicalRoot.GetAllDescendants(robotFilter);

        List<string> resourceNames = new List<string>();

        foreach (var dec in robots)
        {
            var t1 = dec as ITxPlcLogicResource;

            if (t1 != null /*&& t1.LogicBehavior == null*/)
            {
                //output.WriteLine(dec.Name);
                resourceNames.Add(dec.Name);
            }
        }

        if (resourceNames.Count == 0)
        {
            output.WriteLine("No Logic Resources found.");
            return;
        }
        string selected = resourceName;
        //string selected = Path.GetFileNameWithoutExtension(filePath); ;
        //string selected = ShowSelectionDialog(resourceNames);
        output.WriteLine("+++++++ Resource Name " + resourceName);
        if (!string.IsNullOrEmpty(selected))
        {
            output.WriteLine("User selected: {selected}");
        }
        if (selected == "New")
        {

        }
        else
        {
            ITxObject selObjects = null;

            foreach (var dec in robots)
            {
                var t1 = dec as ITxPlcLogicResource;

                if (dec.Name == selected.Trim())
                {
                    selObjects = dec;
                    if (t1.LogicBehavior != null)
                    {
                        MessageBox.Show("logic resource Already exist");
                        return;
                    }

                    break;
                }
            }

            string logicResourceName = selObjects.Name /*+ "_LB"*/;

            //LOGIC_RESOURCE_NAME = logicResourceName;
            // Get the logic resource if exists already such by the given name (alternatively can be used GetLogicResourceByName_V2 method). 
            ITxPlcLogicResource existingLogicResource = GetLogicResourceByName_V1(logicResourceName);
            //////if (existingLogicResource != null)
            //////{
            //////    // Get the ITxPlcLogicBehaviorCreation interface if the aim is to create LogicBehavior for it(there is separate example how to create LogicBehavior for an existing resource).
            //////    ITxPlcLogicBehaviorCreation lbCreation = existingLogicResource as ITxPlcLogicBehaviorCreation;
            //////    output.WriteLine(string.Format("The logic resource '{0}' already exists, you can create logic behavior for it using its ITxPlcLogicBehaviorCreation interface", lbCreation.Name));
            //////    return;
            //////}

            // If the logic resource is not found -> create a new one (example is for eMS/eMS Disc platform).

            // Determine the parent of the new logic resource (if it doesn't exist already) from the pre-selection:
            // - if no pre-selection, the logic resource will be created directly under the physical root (appears last within the 'Resources' folder)
            // - in case of 1 object pre-selection (compound resource/equipment in modeling/a sub-folder of 'Resources'), the logic resource will be moved under it

            //var selObjects = TxApplication.ActiveSelection.GetItems();

            //foreach (var dec in robots)
            //{
            //    var t1 = dec as ITxPlcLogicResource;

            //    if (dec.Name == selected)
            //    {
            //        selObjects = dec;
            //        break;
            //    }
            //}

            ITxObjectCollection parent = null;

            // In case of missing pre-selection - the assumed parent of a new logic resource will be the physical root.
            if (selObjects == null)
            {
                parent = TxApplication.ActiveDocument.PhysicalRoot;
            }
            else if (selObjects != null)
            {
                parent = selObjects as ITxObjectCollection;
                if (parent == null)
                {
                    output.WriteLine("Expected to be pre-selected a single parent under which to create the new logic resource (if it doesn't exist already)");
                    return;
                }
            }
            else
            {
                output.WriteLine("Expected to not have pre-selection or to select a single parent under which to create the new logic resource (if it doesn't exist already)");
                return;
            }

            // Create the resource under the given parent
            //////ITxPlcLogicResource createdLogicResource = CreateLogicResource_eMS(parent, logicResourceName);
            //////if (createdLogicResource != null)
            //////{
            //////    // Get the ITxPlcLogicBehaviorCreation interface if the aim is to create LogicBehavior for it(there is separate example how to create LogicBehavior for an existing resource).
            //////    ITxPlcLogicBehaviorCreation lbCreation = createdLogicResource as ITxPlcLogicBehaviorCreation;
            //////    output.WriteLine(string.Format("Under '{0}' is created a new logic resource '{1}' , you can create logic behavior for it using its ITxPlcLogicBehaviorCreation interface", parent.Name, lbCreation.Name));
            //////}


            ITxComponent targetResource = null;

            // Get the current selection.
            var selItems = selObjects;


            if (selItems != null)
            {
                // Accept single Resource selection, which doesn't have Logic Behavior yet.
                var selResource = selItems as ITxPlcLogicResource;

                output.WriteLine(selResource.ToString());
                if (selResource == null || selResource.LogicBehavior != null)
                {
                    output.WriteLine("Selected object must be a resource which doesn't have Logic Behavior yet.");
                    output.WriteLine("No action is done!");
                    return;
                }

                // Assign the target resource to be the selected one.
                targetResource = selResource as ITxComponent;
            }
            else// Consider creating a new resource, for which afterwards a Logic Behavior will be created.
            {
                output.WriteLine("nothing will be done!");
                return;
            }

            // Create Logic Behavior on the target resource.
            TxPlcLogicBehavior lb = CreateLogicBehavior_OnResource(targetResource);
            output.WriteLine(string.Format("Logic Behavior successfully created for '{0}'!", targetResource.Name));

            // Refresh the display after creating of new objects.
            TxApplication.RefreshDisplay();

            ITxObject logicResources = selObjects;// TxApplication.ActiveSelection.GetFilteredItems(new TxTypeFilter(typeof(ITxPlcLogicResource)));
            if (logicResources == null)
            {
                output.WriteLine("Unsupported selection - expected single resource with Logic Behavior in modeling. a");
                return;
            }


            ITxPlcLogicResource logicResource = logicResources as ITxPlcLogicResource;
            if (!(logicResource as ITxComponent).IsOpenForModeling)
            {
                output.WriteLine("Unsupported selection - expected single resource with Logic Behavior in modeling. b");
                return;
            }

            TxPlcLogicBehavior logicBehavior = logicResource.LogicBehavior;
            if (logicBehavior == null)
            {
                output.WriteLine("Unsupported selection - expected single resource with Logic Behavior in modeling. c");
                return;
            }


            // Create entries "A" & "B" needed for some of the expression examples.
            //ITxPlcLogicBehaviorEntry A = CreateEntry(logicBehavior, "A", TxPlcHardwareType.Int);
            //ITxPlcLogicBehaviorEntry B = CreateEntry(logicBehavior, "B", TxPlcHardwareType.Int);
            int i = 0;

            try
            {
                foreach (var parameter3 in paramList)
                {
                    if (parameter3[0] != "")
                    {
                        //TxPlcLogicBehaviorExpressionValueParameter A1 = CreateParameterExp(logicBehavior, parameter3[0], TxPlcHardwareType.Bool);

                        //if (parameter3[2] != "")
                        //    A1.Expression = CreateExpression(parameter3[2], logicBehavior, ref output1);
                    }
                }

            }
            catch
            {

            }
            try
            {
                foreach (var parameter1 in jointdistanceparameterList)
                {
                    if (parameter1[0] != "")
                    {
                        ITxPlcLogicBehaviorParameter jvs_j11 = CreateJointDistanceValueParameterUpd(logicBehavior, (logicResource as ITxDevice).DrivingJoints[i] as TxJoint, TxPlcHardwareType.Real, parameter1[0], ref output1);
                        i++;
                    }
                }

            }
            catch
            {

            }
            i = 0;

            List<string> PoseList = new List<string>();

            ITxDevice device = logicResources as ITxDevice;
            //TxObjectList poses = logicResource.PoseList;
            TxObjectList poses = device.PoseList;
            for (int i4 = 0; i4 < poses.Count; i4++)
            {
                var poseObj = poses[i];
                string poseName = "<unknown>";
                try
                {
                    poseName = poseObj.GetType().GetProperty("Name").GetValue(poseObj, null).ToString();
                }
                catch { }

                //WriteCsvLine(writer, device.Name, "Pose", poseName);
                PoseList.Add(poseName);
            }

            try
            {
                foreach (var parameter2 in atposeparameterList)
                {
                    if (parameter2[0] != "")
                    {
                        for (int i1 = 0; i1 < PoseList.Count; i1++)
                        {
                            if (parameter2[3].Trim() == PoseList[i1].Trim())
                            {
                                TxObjectList poses11 = (logicResource as ITxDevice).PoseList;
                                TxPose pose1 = poses11[i1] as TxPose;

                                ITxPlcLogicBehaviorParameter jvs_j11 = CreateJointValueSensorParameter(logicBehavior, pose1, parameter2[0], ref output1);
                            }
                        }

                    }
                }

            }
            catch
            {

            }
            List<string> jointList = new List<string>();
            TxObjectList joints = device.DrivingJoints;
            for (int i8 = 0; i8 < joints.Count; i8++)
            {
                string jointName = "<unknown>";
                try
                {
                    jointName = joints[i8].GetType().GetProperty("Name").GetValue(joints[i8], null).ToString();
                }
                catch { }

                jointList.Add(jointName);
            }



            //try
            //{
            //    foreach (var parameter in ParameterData)
            //    {
            //        //output.WriteLine(parameter[1] + " , " + parameter[0]);
            //        if (parameter[1] == "Joint_Distance_Sensor")
            //        {
            //            try
            //            {
            //                ITxPlcLogicBehaviorParameter jvs_j11 = CreateJointDistanceValueParameterUpd(logicBehavior, (logicResource as ITxDevice).DrivingJoints[i] as TxJoint, TxPlcHardwareType.Real, parameter[0], ref output1);
            //                i++;
            //            }
            //            catch (Exception exx2)
            //            {
            //                output.WriteLine("Error For Jiont Distance Sensor: " + exx2.Message);
            //            }
            //        }
            //        else if (parameter[1] == "Joint_Value_Sensor")
            //        {
            //            if (parameter[5] == "Range" && parameter[3] == "Pose")
            //            {
            //                try
            //                {
            //                    TxObjectList poses = (logicResource as ITxDevice).PoseList;


            //                    if (parameter[4] == "P0")
            //                    {
            //                        TxPose pose1 = poses[1] as TxPose;
            //                        ITxPlcLogicBehaviorParameter jvs_j11 = CreateJointValueSensorParameter(logicBehavior, pose1, parameter[0], ref output1);
            //                    }
            //                    else if (parameter[4] == "P180")
            //                    {
            //                        TxPose pose1 = poses[2] as TxPose;
            //                        ITxPlcLogicBehaviorParameter jvs_j11 = CreateJointValueSensorParameter(logicBehavior, pose1, parameter[0], ref output1);


            //                    }

            //                }
            //                catch (Exception exx2)
            //                {
            //                    output.WriteLine("Error For Jiont Distance Sensor: " + exx2.Message);
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception exx1)
            //{
            //    output.WriteLine("Error For Jiont Distance Sensor: " + exx1.Message);
            //}

            try
            {
                if (constantList.Count != 0)
                {
                    foreach (var constant in constantList)
                    {
                        if (constant[0] != "")
                        {
                            TxPlcValue val = new TxPlcValue();

                            if (constant[1] == "Real")
                            {
                                float number = float.Parse(constant[2], CultureInfo.InvariantCulture);
                                val.RealValue = number;

                                ITxPlcLogicBehaviorConstant createdConst = CreateConstant(logicBehavior, constant[0], TxPlcHardwareType.Real, val);
                            }
                            //TxPlcValue val = new TxPlcValue();

                            //if (constant[1] == "Real")
                            //{

                            //    double number = double.Parse(constant[2], CultureInfo.InvariantCulture);
                            //    //double number = Convert.ToDouble(constant[2]);
                            //    val.RealValue = number;

                            //    ITxPlcLogicBehaviorConstant createdConst = CreateConstant(logicBehavior, constant[0], TxPlcHardwareType.Real, val);
                            //}
                            else if (constant[1] == "bool")
                            {
                                float number = Convert.ToSingle(constant[2]);
                                val.RealValue = number;

                                ITxPlcLogicBehaviorConstant createdConst = CreateConstant(logicBehavior, constant[0], TxPlcHardwareType.Bool, val);
                            }
                            else if (constant[1] == "byte")
                            {
                                float number = Convert.ToSingle(constant[2]);
                                val.RealValue = number;

                                ITxPlcLogicBehaviorConstant createdConst = CreateConstant(logicBehavior, constant[0], TxPlcHardwareType.Byte, val);
                            }
                        }
                    }
                }
                else
                {
                    output.WriteLine("Empty Constant list");
                }
            }
            catch (Exception ex)
            {
                output.WriteLine("Constant Creation Error: " + ex.Message);
            }

            foreach (var entry in entryList)
            {
                try
                {
                    if (entry[1].ToLower() == "bool")
                    {
                        ITxPlcLogicBehaviorEntry A = CreateEntry(logicBehavior, entry[0], TxPlcHardwareType.Bool);

                    }
                }
                catch (Exception ex)
                {
                    output.WriteLine("Error For Create Entry :" + ex.Message);
                }
            }

            // Run the examples - create a new exit for any of the examples + assign the expression.

            // Example 1 - simple value expression : TRUE

            foreach (var exit in exitList)
            {
                try
                {
                    if (exit[1].ToLower() == "bool")
                    {
                        ITxPlcLogicBehaviorExit exit1 = CreateExit(logicBehavior, exit[0], TxPlcHardwareType.Bool);
                        //exit1.expression =
                        if (!string.IsNullOrEmpty(exit[8]))
                        {
                            exit1.Expression = CreateExpression(exit[8], logicBehavior, ref output1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    output.WriteLine("Error For Create Exit: " + ex.Message);
                }
            }

            //output.WriteLine("Logic Behavior is succefully updated with the example expressions!");

            //try
            //{
            //    foreach (var exitData in ExitTags)
            //    {
            //        output.WriteLine(exitData);
            //        foreach (var exit in logicBehavior.Exits)
            //        {
            //            if (exit.Name == exitData[0].Split('_')[1])
            //            {
            //            }
            //          }
            //    }
            //}
            //catch(Exception exx5) 
            //{
            //    output.WriteLine(exx5.Message);
            //}
            // Create parameter "A" of type BOOL.
            ////ITxPlcLogicBehaviorParameter A1 = CreateParameter(logicBehavior, "A1", TxPlcHardwareType.Bool);
            // Create parameter "B" of type REAL.
            ////ITxPlcLogicBehaviorParameter B1 = CreateParameter(logicBehavior, "B1", TxPlcHardwareType.Real);

            //foreach (var t in ActionData)
            //{
            //    output.WriteLine(t[3]);
            //}

            //output.WriteLine("Logic Behavior is succefully updated with the example parameters!");

            try
            {
                if (movposList.Count != 0)
                {
                    foreach (var action in movposList)
                    {
                        if (action[0] != "")
                        {
                            try
                            {
                                for (int i3 = 0; i3 < PoseList.Count; i3++)
                                {
                                    if (PoseList[i3].Trim() == action[2].Trim())
                                    {
                                        TxObjectList poses11 = (logicResource as ITxDevice).PoseList;
                                        TxPlcLogicBehaviorMoveToPoseAction mpAction = null;
                                        TxPose pose1 = poses11[i3] as TxPose;
                                        // Example 6 - create 'MOVE TO POSE' x`action
                                        mpAction = CreateMoveToPoseAction(logicBehavior, pose1, action[0]);

                                        if (action[4] != "")
                                        {
                                            output.WriteLine(action[4]);
                                            string numberString = action[4];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            mpAction.VelocityExpression = CreateSignedValueExpression(number);
                                        }
                                        if (action[3] != "")
                                            mpAction.Expression = CreateExpression(action[3], logicBehavior, ref output1);

                                        break;

                                    }
                                }

                            }
                            catch (Exception Exx)
                            {
                                output.WriteLine("Error For Release Action: " + Exx.Message);
                                //output.WriteLine("Release Err");
                            }

                        }

                    }
                }
            }
            catch
            {
            }

            try
            {
                if (jointVelocityControlled.Count != 0)
                {
                    foreach (var action in jointVelocityControlled)
                    {
                        if (action[0] != "")
                        {
                            try
                            {
                                for (int i4 = 0; i4 < jointList.Count; i4++)
                                {
                                    if (jointList[i4].Trim() == action[2].Trim())
                                    {
                                        TxObjectList drivingJoints1 = (logicResource as ITxDevice).DrivingJoints;
                                        TxJoint j11 = drivingJoints1[i4] as TxJoint;

                                        TxPlcLogicBehaviorJointVelocityControlledAction jvcAction = CreateJointVelocityControlledAction(logicBehavior, j11, action[0]);

                                        //if (action[4] != "")
                                        //{
                                        //    output.WriteLine(action[4]);
                                        //    string numberString = action[4];
                                        //    int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                        //    jvcAction.VelocityExpression = CreateSignedValueExpression(number);
                                        //}
                                        if (action[3] != "")
                                            jvcAction.Expression = CreateExpression(action[3], logicBehavior, ref output1);
                                        if (action[4] != "")
                                        {
                                            output.WriteLine(action[4]);
                                            string numberString = action[4];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            jvcAction.TargetVelocityExpression = CreateSignedValueExpression(number);
                                        }
                                        if (action[5] != "")
                                        {
                                            output.WriteLine(action[5]);
                                            string numberString = action[5];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            jvcAction.AccelerationExpression = CreateSignedValueExpression(number);
                                        }
                                        if (action[6] != "")
                                        {
                                            output.WriteLine(action[6]);
                                            string numberString = action[6];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            jvcAction.DecelerationExpression = CreateSignedValueExpression(number);
                                        }
                                        break;

                                    }
                                }

                            }
                            catch (Exception Exx)
                            {
                                output.WriteLine("Error For Release Action: " + Exx.Message);
                                //output.WriteLine("Release Err");
                            }

                        }

                    }
                }
            }
            catch
            {
            }
            try
            {
                if (movetojointVal.Count != 0)
                {
                    foreach (var action in movetojointVal)
                    {
                        if (action[0] != "")
                        {
                            try
                            {
                                for (int i4 = 0; i4 < jointList.Count; i4++)
                                {
                                    if (jointList[i4].Trim() == action[2].Trim())
                                    {
                                        TxObjectList drivingJoints1 = (logicResource as ITxDevice).DrivingJoints;
                                        TxJoint j11 = drivingJoints1[i4] as TxJoint;

                                        TxPlcLogicBehaviorMoveJointToValueAction mjvAction = CreateMoveJointToValueAction(logicBehavior, j11, action[0]); ;

                                        //if (action[4] != "")
                                        //{
                                        //    output.WriteLine(action[4]);
                                        //    string numberString = action[4];
                                        //    int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                        //    jvcAction.VelocityExpression = CreateSignedValueExpression(number);
                                        //}
                                        if (action[3] != "")
                                            mjvAction.Expression = CreateExpression(action[3], logicBehavior, ref output1);

                                        if (action[4] != "")
                                            mjvAction.Expression = CreateExpression(action[4], logicBehavior, ref output1);

                                        if (action[5] != "")
                                        {
                                            output.WriteLine(action[4]);
                                            string numberString = action[4];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            mjvAction.TargetVelocityExpression = CreateSignedValueExpression(number);
                                        }
                                        if (action[6] != "")
                                        {
                                            output.WriteLine(action[5]);
                                            string numberString = action[5];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            mjvAction.AccelerationExpression = CreateSignedValueExpression(number);
                                        }
                                        if (action[7] != "")
                                        {
                                            output.WriteLine(action[6]);
                                            string numberString = action[6];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            mjvAction.DecelerationExpression = CreateSignedValueExpression(number);
                                        }
                                        break;

                                    }
                                }


                            }
                            catch (Exception Exx)
                            {
                                output.WriteLine("Error For Release Action: " + Exx.Message);
                                //output.WriteLine("Release Err");
                            }

                        }

                    }
                }
            }
            catch
            {
            }
            try
            {
                if (jointaccelerationControlledAction.Count != 0)
                {
                    foreach (var action in jointaccelerationControlledAction)
                    {
                        if (action[0] != "")
                        {
                            try
                            {
                                for (int i4 = 0; i4 < jointList.Count; i4++)
                                {
                                    if (jointList[i4].Trim() == action[2].Trim())
                                    {
                                        TxObjectList drivingJoints1 = (logicResource as ITxDevice).DrivingJoints;
                                        TxJoint j11 = drivingJoints1[i4] as TxJoint;
                                        TxPlcLogicBehaviorJointAccelerationControlledAction jacAction = CreateJointAccelerationControlledAction(logicBehavior, j11, action[0]);

                                        if (action[3] != "")
                                            jacAction.Expression = CreateExpression(action[3], logicBehavior, ref output1);
                                        if (action[4] != "")
                                        {
                                            output.WriteLine(action[4]);
                                            string numberString = action[4];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            jacAction.TargetVelocityDirectionExpression = CreateSignedValueExpression(number);
                                        }
                                        if (action[5] != "")
                                        {
                                            output.WriteLine(action[5]);
                                            string numberString = action[5];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            jacAction.AccelerationExpression = CreateSignedValueExpression(number);
                                        }
                                        if (action[6] != "")
                                        {
                                            output.WriteLine(action[6]);
                                            string numberString = action[6];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            jacAction.DecelerationExpression = CreateSignedValueExpression(number);
                                        }
                                    }
                                }

                            }
                            catch (Exception Exx)
                            {
                                output.WriteLine("Error For Release Action: " + Exx.Message);
                                //output.WriteLine("Release Err");
                            }

                        }

                    }
                }
            }
            catch
            {
            }

            try
            {
                if (jumpjointactionList.Count != 0)
                {
                    foreach (var action in jumpjointactionList)
                    {
                        if (action[0] != "")
                        {
                            try
                            {
                                for (int i4 = 0; i4 < jointList.Count; i4++)
                                {
                                    if (jointList[i4].Trim() == action[2].Trim())
                                    {
                                        TxObjectList drivingJoints1 = (logicResource as ITxDevice).DrivingJoints;
                                        TxJoint j11 = drivingJoints1[i4] as TxJoint;
                                        TxPlcLogicBehaviorJumpJointAction jjAction = CreateJumpJointAction(logicBehavior, j11, action[0]);

                                        if (action[3] != "")
                                            jjAction.Expression = CreateExpression(action[3], logicBehavior, ref output1);
                                        if (action[4] != "")
                                        {
                                            output.WriteLine(action[4]);
                                            string numberString = action[4];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            jjAction.ValueExpression = CreateSignedValueExpression(number);
                                        }

                                    }
                                }

                            }
                            catch (Exception Exx)
                            {
                                output.WriteLine("Error For Release Action: " + Exx.Message);
                                //output.WriteLine("Release Err");
                            }

                        }

                    }
                }
            }
            catch
            {
            }
            try
            {
                if (movposList.Count != 0)
                {
                    foreach (var action in movposList)
                    {
                        if (action[0] != "")
                        {
                            try
                            {
                                for (int i3 = 0; i3 < PoseList.Count; i3++)
                                {
                                    if (PoseList[i3].Trim() == action[2].Trim())
                                    {
                                        TxObjectList poses13 = (logicResource as ITxDevice).PoseList;
                                        TxPlcLogicBehaviorMoveToPoseAction mpAction = null;
                                        TxPose pose1 = poses13[i3] as TxPose;
                                        // Example 6 - create 'MOVE TO POSE' x`action
                                        mpAction = CreateMoveToPoseAction(logicBehavior, pose1, action[0]);

                                        if (action[4] != "")
                                        {
                                            output.WriteLine(action[4]);
                                            string numberString = action[4];
                                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                            mpAction.VelocityExpression = CreateSignedValueExpression(number);
                                        }
                                        if (action[3] != "")
                                            mpAction.Expression = CreateExpression(action[3], logicBehavior, ref output1);

                                        break;

                                    }
                                }

                            }
                            catch (Exception Exx)
                            {
                                output.WriteLine("Error For Release Action: " + Exx.Message);
                                //output.WriteLine("Release Err");
                            }

                        }

                    }
                }
            }
            catch
            {
            }
            //Add Actions

            try
            {
                if (gripactionList.Count != 0)
                {
                    foreach (var action in gripactionList)
                    {
                        if (action[0] != "")
                        {
                            try
                            {
                                ITxPlcLogicBehaviorAction gripAction = CreateGripActionUpd(logicBehavior, action[0], action[2], ref output1);
                                output.WriteLine(output1.ToString());
                                if (action[3] != "")
                                    gripAction.Expression = CreateExpression(action[3], logicBehavior, ref output1);

                            }
                            catch (Exception exx)
                            {
                                output.WriteLine("Error For Grip Action: " + exx.Message);
                                //output.WriteLine("Error For Grip Action");
                            }

                        }

                    }
                }
            }
            catch
            {
            }


            try
            {
                if (releaseactionList.Count != 0)
                {
                    foreach (var action in releaseactionList)
                    {
                        if (action[0] != "")
                        {
                            try
                            {
                                ITxPlcLogicBehaviorAction releaseAction1 = CreateReleaseActionUpd(logicBehavior, action[0], action[2]);
                                if (action[3] != "")
                                    releaseAction1.Expression = CreateExpression(action[3], logicBehavior, ref output1);
                            }
                            catch (Exception Exx)
                            {
                                output.WriteLine("Error For Release Action: " + Exx.Message);
                                //output.WriteLine("Release Err");
                            }

                        }

                    }
                }
            }
            catch
            {
            }

            //move to pose
            //Jump joint action

            try
            {
                if (movjointList.Count != 0)
                {
                    foreach (var action in movjointList)
                    {
                        if (action[0] != "")
                        {
                            try
                            {
                                for (int i2 = 0; i2 < jointList.Count; i2++)
                                {
                                    if (jointList[i2] == action[2])
                                    {
                                        TxObjectList drivingJoints1 = (logicResource as ITxDevice).DrivingJoints;
                                        TxJoint j11 = drivingJoints1[i2] as TxJoint;
                                        TxPlcLogicBehaviorMoveJointAction mjAction1 = CreateMoveJointActionUpd(logicBehavior, j11, action[0]);

                                        if (action[3] != "")
                                            mjAction1.Expression = CreateExpression(action[3], logicBehavior, ref output1);

                                        //if (action[4] != "")
                                        //{
                                        //    //"""-40"""
                                        //    output.WriteLine(action[4]);
                                        //    string numberString = action[4];
                                        //    int number = int.Parse(numberString, CultureInfo.InvariantCulture);
                                        //    mjAction1.VelocityExpression = CreateSignedValueExpression(number);
                                        //}

                                        if (action[4] != null && action[4].Trim() != "")
                                        {
                                            string rawValue = action[4];
                                            output.WriteLine("Raw: " + rawValue);

                                            // Remove double quotes and trim spaces
                                            string cleanedValue = rawValue.Replace("\"", "").Trim();

                                            int number = 0;
                                            bool success = int.TryParse(cleanedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out number);

                                            if (success)
                                            {
                                                mjAction1.VelocityExpression = CreateSignedValueExpression(number);
                                            }
                                            else
                                            {
                                                output.WriteLine("Warning: Unable to parse numeric value from '" + rawValue + "'");
                                            }
                                        }


                                        break;
                                    }
                                }
                            }
                            catch
                            {

                            }

                        }

                    }
                }
            }
            catch
            {
            }


            //try
            //{
            //    if (ActionData.Count != 0)
            //    {
            //        foreach (var action in ActionData)
            //        {
            //            if (action[1] == "Grip")
            //            {
            //                try
            //                {
            //                    ITxPlcLogicBehaviorAction gripAction = CreateGripActionUpd(logicBehavior, action[0], action[2], ref output1);
            //                    output.WriteLine(output1.ToString());
            //                    gripAction.Expression = CreateExpression(action[3], logicBehavior, ref output1);
            //                }
            //                catch (Exception exx)
            //                {
            //                    output.WriteLine("Error For Grip Action: " + exx.Message);
            //                    //output.WriteLine("Error For Grip Action");
            //                }

            //            }
            //            else if (action[1] == "Release")
            //            {
            //                try
            //                {
            //                    ITxPlcLogicBehaviorAction releaseAction1 = CreateReleaseActionUpd(logicBehavior, action[0], action[2]);
            //                    releaseAction1.Expression = CreateExpression(action[3], logicBehavior, ref output1);
            //                }
            //                catch (Exception Exx)
            //                {
            //                    output.WriteLine("Error For Release Action: " + Exx.Message);
            //                    //output.WriteLine("Release Err");
            //                }
            //            }
            //            else if (action[1] == "Move_joint")
            //            {
            //                output.WriteLine("Move1");
            //                try
            //                {
            //                    //TxObjectList drivingJoints1 = null;
            //                    //TxJoint j11 = null;

            //                    if (action[2] == "j0")
            //                    {
            //                        output.WriteLine("Move2");
            //                        TxObjectList drivingJoints1 = (logicResource as ITxDevice).DrivingJoints;
            //                        TxJoint j11 = drivingJoints1[0] as TxJoint;
            //                        TxPlcLogicBehaviorMoveJointAction mjAction1 = CreateMoveJointActionUpd(logicBehavior, j11, action[0]);
            //                        //mjAction1.VelocityExpression = CreateSignedValueExpression(40);

            //                        if (action[4] != "")
            //                        {
            //                            output.WriteLine(action[4]);
            //                            string numberString = action[4];
            //                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
            //                            mjAction1.VelocityExpression = CreateSignedValueExpression(number);
            //                        }
            //                        mjAction1.Expression = CreateExpression(action[3], logicBehavior, ref output1);
            //                    }
            //                    else if (action[2] == "Grip1")
            //                    {
            //                        TxObjectList drivingJoints1 = (logicResource as ITxDevice).DrivingJoints;
            //                        TxJoint j11 = drivingJoints1[1] as TxJoint;
            //                        TxPlcLogicBehaviorMoveJointAction mjAction1 = CreateMoveJointActionUpd(logicBehavior, j11, action[0]);
            //                        //mjAction1.VelocityExpression = CreateSignedValueExpression(40);

            //                        if (action[4] != "")
            //                        {
            //                            //output.WriteLine(action[4]);
            //                            string numberString = action[4];
            //                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
            //                            mjAction1.VelocityExpression = CreateSignedValueExpression(number);
            //                        }
            //                        mjAction1.Expression = CreateExpression(action[3], logicBehavior, ref output1);
            //                    }
            //                    else if (action[2] == "Grip2")
            //                    {
            //                        TxObjectList drivingJoints1 = (logicResource as ITxDevice).DrivingJoints;
            //                        TxJoint j11 = drivingJoints1[2] as TxJoint;
            //                        TxPlcLogicBehaviorMoveJointAction mjAction1 = CreateMoveJointActionUpd(logicBehavior, j11, action[0]);
            //                        //mjAction1.VelocityExpression = CreateSignedValueExpression(40);

            //                        if (action[4] != "")
            //                        {
            //                            //output.WriteLine(action[4]);
            //                            string numberString = action[4];
            //                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
            //                            mjAction1.VelocityExpression = CreateSignedValueExpression(number);
            //                        }
            //                        mjAction1.Expression = CreateExpression(action[3], logicBehavior, ref output1);
            //                    }
            //                    else if (action[2] == "Grip3")
            //                    {
            //                        TxObjectList drivingJoints1 = (logicResource as ITxDevice).DrivingJoints;
            //                        TxJoint j11 = drivingJoints1[3] as TxJoint;
            //                        TxPlcLogicBehaviorMoveJointAction mjAction1 = CreateMoveJointActionUpd(logicBehavior, j11, action[0]);
            //                        //mjAction1.VelocityExpression = CreateSignedValueExpression(40);

            //                        if (action[4] != "")
            //                        {
            //                            //output.WriteLine(action[4]);
            //                            string numberString = action[4];
            //                            int number = int.Parse(numberString, CultureInfo.InvariantCulture);
            //                            mjAction1.VelocityExpression = CreateSignedValueExpression(number);
            //                        }
            //                        mjAction1.Expression = CreateExpression(action[3], logicBehavior, ref output1);
            //                    }

            //                }
            //                catch (Exception exx)
            //                {
            //                    output.WriteLine("Error For Move Joint Action: " + exx.Message);
            //                    //output.WriteLine("Move Joint Err");
            //                }
            //            }
            //            else if (action[1] == "Move_to_pose")
            //            {
            //                TxObjectList poses = (logicResource as ITxDevice).PoseList;
            //                TxPlcLogicBehaviorMoveToPoseAction mpAction = null;
            //                if (action[2] == "P0")
            //                {
            //                    TxPose pose1 = poses[1] as TxPose;
            //                    // Example 6 - create 'MOVE TO POSE' x`action
            //                    mpAction = CreateMoveToPoseAction(logicBehavior, pose1, action[0]);

            //                }
            //                else if (action[2] == "P180")
            //                {
            //                    TxPose pose1 = poses[2] as TxPose;
            //                    // Example 6 - create 'MOVE TO POSE' action
            //                    mpAction = CreateMoveToPoseAction(logicBehavior, pose1, action[0]);

            //                }

            //                if (action[4] != "")
            //                {
            //                    //output.WriteLine(action[4]);
            //                    string numberString = action[4];
            //                    int number = int.Parse(numberString, CultureInfo.InvariantCulture);
            //                    mpAction.VelocityExpression = CreateSignedValueExpression(number);
            //                }
            //                mpAction.Expression = CreateExpression(action[3], logicBehavior, ref output1);


            //                output.WriteLine("Logic Behavior is succefully updated with the example joint + pose actions!");
            //            }
            //        }
            //    }
            //    else
            //    {
            //        output.WriteLine("Empty Grip list");
            //    }
            //}
            //catch (Exception exx7)
            //{
            //    output.WriteLine("Error For Action Creation: " + exx7.Message);
            //}


            //Add constant



            //Add signal to entry
            try
            {
                foreach (var entryData in entryList)
                {

                    foreach (var entry in logicBehavior.Entries)
                    {
                        try
                        {

                            if (entry.Name == entryData[0])

                            {
                                if (entryData[1].Trim() == "Bool")
                                {
                                    output.WriteLine("Entered");
                                    //string[] parts = entryData[2].Split('.');
                                    //string first = parts[0];
                                    //string second = (parts.Length > 1) ? parts[1] : "0";

                                    int number1 = -1;//int.Parse(first, CultureInfo.InvariantCulture);
                                    int number2 = -1;//int.Parse(second, CultureInfo.InvariantCulture);

                                    //string first = entryData[2].Split('.')[0];
                                    //string second = entryData[2].Split('.')[1];

                                    //int number1 = int.Parse(first, CultureInfo.InvariantCulture);
                                    //int number2 = int.Parse(second, CultureInfo.InvariantCulture);

                                    ITxPlcSignal signalA = GetOrCreateOutputSignal(entryData[2].Trim(), TxPlcSignalDataType.Bool, number1, number2, "PLC");
                                    ConnectSignalToEntry(entry, signalA, output);


                                    if (!string.IsNullOrEmpty(entryData[3]))
                                    {
                                        TxPlcFromRobotSignal signalRobot = GetExistingRobotSignal(entryData[3], TxPlcSignalDataType.Bool);
                                        if (signalRobot != null)
                                        {
                                            ConnectRobotSignalToEntry(entry, signalRobot, output);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exc4)
                        {
                            output.WriteLine("Error For Entry Signal Connection: " + exc4.Message);
                        }
                    }
                }
            }
            catch (Exception ex3)
            {
                output.WriteLine("Error For Entry Signal Connection: " + ex3.Message);
            }


            //Add signal to exit
            try
            {
                foreach (var exitData in exitList)
                {
                    //output.WriteLine(exitData);
                    foreach (var exit in logicBehavior.Exits)
                    {


                        if (exit.Name == exitData[0])
                        {
                            if (exitData[2] != "")
                            {
                                if (exitData[1].Trim() == "Bool")
                                {
                                    output.WriteLine("Entered Exit");
                                    output.WriteLine(exitData[0] + " ," + exitData[1] + " ," + exitData[2]);
                                    //string[] parts = exitData[2].Split('.');
                                    //string first = parts[0];
                                    //string second = (parts.Length > 1) ? parts[1] : "0";

                                    int number1 = -1;// int.Parse(first, CultureInfo.InvariantCulture);
                                    int number2 = -1; //int.Parse(second, CultureInfo.InvariantCulture);
                                                      //string first = exitData[2].Split('.')[0];
                                                      //string second = exitData[2].Split('.')[1];

                                    //int number1 = int.Parse(first, CultureInfo.InvariantCulture);
                                    //int number2 = int.Parse(second, CultureInfo.InvariantCulture);

                                    ITxPlcSignal signalExit = GetOrCreateInputSignal(exitData[2].Trim(), TxPlcSignalDataType.Bool, number1, number2, "PLC");
                                    ConnectSignalToExit(exit, signalExit, output);


                                    if (!string.IsNullOrEmpty(exitData[3]))
                                    {
                                        TxPlcToRobotSignal signalRobot = GetExistingRobotSignalTo(exitData[3], TxPlcSignalDataType.Bool);
                                        if (signalRobot != null)
                                        {
                                            ConnectRobotSignalToExit(exit, signalRobot, output);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex3)
            {
                output.WriteLine("Error For Exit Signal Connection: " + ex3.Message);
            }
            //try
            //{
            //    if (ConstantData.Count != 0)
            //    {
            //        foreach (var constant in ConstantData)
            //        {
            //            if (constant[1] !="")
            //            {
            //                TxPlcHardwareType hardwareType = new HardwareType();
            //                //CreateConstant(logicBehavior, constant[0], TxPlcHardwareType hardwareType, TxPlcValue value)
            //                if (constant[1] == "Real")
            //                {
            //                    TxPlcValue val = new TxPlcValue();
            //                    float number = Convert.ToSingle(constant[2]);
            //                   val.RealValue = number;
            //                    ITxPlcLogicBehaviorConstant constant = CreateConstant(TxPlcLogicBehavior lb, constant[0], TxPlcHardwareType.Real, val);
            //                } 
            //            }

            //        }
            //    }
            //    else
            //    {
            //        output.WriteLine("Grip list empty");
            //    }
            //}
            //catch
            //{
            //    output.WriteLine("Grip Error");
            //}

            output.WriteLine(output1.ToString());
            //TxObjectList logicResources = logicBehavior as ITxGripper;

            //// Example 1 - create GRIP action
            //ITxPlcLogicBehaviorAction gripAction1 = CreateGripAction(logicBehavior, "GRIP");
            //// Example 2 - create RELEASE action
            //ITxPlcLogicBehaviorAction releaseAction = CreateReleaseAction(logicBehavior, "RELEASE");

            //output.WriteLine("Logic Behavior is succefully updated with the example grip/release actions!");


            TxObjectList drivingJoints = (logicResource as ITxDevice).DrivingJoints;
            TxJoint j1 = drivingJoints[0] as TxJoint;
            int example = 1;

            switch (example)
            {
                case 1: // Example 1 - create 'MOVE JOINT TO VALUE' action
                    //TxPlcLogicBehaviorMoveJointToValueAction mjvAction = CreateMoveJointToValueAction(logicBehavior, j1, "MJV");
                    break;
                case 2: // Example 2 - create 'MOVE JOINT' action
                    //TxPlcLogicBehaviorMoveJointAction mjAction = CreateMoveJointAction(logicBehavior, j1, "MJ");
                    break;
                case 3: // Example 3 - create 'JUMP JOINT' action
                    //TxPlcLogicBehaviorJumpJointAction jjAction = CreateJumpJointAction(logicBehavior, j1, "JJ");
                    break;
                case 4: // Example 4 - create 'JOINT VELOCITY CONTROLLED' action
                    //TxPlcLogicBehaviorJointVelocityControlledAction jvcAction = CreateJointVelocityControlledAction(logicBehavior, j1, "JVC");
                    break;
                case 5: // Example 5 - create 'JOINT ACCELERATION CONTROLLED' action
                    //TxPlcLogicBehaviorJointAccelerationControlledAction jacAction = CreateJointAccelerationControlledAction(logicBehavior, j1, "JAC");
                    break;
            }

            // For the example related to move to action use the 1st found pose usually 'HOME' (specifying pose is mandatory).
            //TxObjectList poses = (logicResource as ITxDevice).PoseList;
            //TxPose pose1 = poses[0] as TxPose;
            // Example 6 - create 'MOVE TO POSE' action	
            //TxPlcLogicBehaviorMoveToPoseAction mpAction = CreateMoveToPoseAction(logicBehavior, pose1, "MP");

            //output.WriteLine("Logic Behavior is succefully updated with the example joint + pose actions!");


            //for (int i = 0; i < 2; i++)
            //{
            //    string paramName = "jds_Extend" + i;

            //    // Check if parameter already exists in logicBehavior
            //    if (!logicBehavior.Parameters.Any(p => p.Name == paramName))
            //    {
            //        var joint = (logicResource as ITxDevice).DrivingJoints[0] as TxJoint;

            //        ITxPlcLogicBehaviorParameter param = CreateJointDistanceValueParameter(
            //            logicBehavior, joint, TxPlcHardwareType.Real, paramName);

            //        output.WriteLine("Created parameter: " + param.Name);
            //    }
            //    else
            //    {
            //        output.WriteLine("Skipped duplicate: " + paramName);
            //    }
            //}
            //ITxPlcLogicBehaviorParameter jds_Extend = CreateJointDistanceValueParameter(logicBehavior, (logicResource as ITxDevice).DrivingJoints[0] as TxJoint, TxPlcHardwareType.Real,"jds_Extend");
            //ITxPlcLogicBehaviorParameter jds_Finger = CreateJointDistanceValueParameter(logicBehavior, (logicResource as ITxDevice).DrivingJoints[1] as TxJoint, TxPlcHardwareType.Real,"jds_Finger");
            //ITxPlcLogicBehaviorParameter jds_Vaccum = CreateJointDistanceValueParameter(logicBehavior, (logicResource as ITxDevice).DrivingJoints[2] as TxJoint, TxPlcHardwareType.Real,"jds_Vaccum");
            //ITxPlcLogicBehaviorParameter jds_Bin = CreateJointDistanceValueParameter(logicBehavior, (logicResource as ITxDevice).DrivingJoints[3] as TxJoint, TxPlcHardwareType.Real,"jds_Bin");

            //ITxPlcLogicBehaviorParameter jvs_j2 CreateJointDistanceSensorParameter(logicBehavior, (logicResource as ITxDevice).DrivingJoints[0] as TxJoint);

            //ITxPlcLogicBehaviorParameter jvs_j1 = CreateJointValueSensorParameter(logicBehavior, (logicResource as ITxDevice).DrivingJoints[0] as TxJoint);

            //foreach (TxPose pose in (logicResource as ITxDevice).PoseList)
            //{
            //    // You can only create pose sensor for a prototype pose (e.g. not an instance pose). 
            //    if (!pose.IsAtInstance)
            //    {

            //        ITxPlcLogicBehaviorParameter jvsPose = CreatePoseSensorParameter(logicBehavior, pose);
            //        break;
            //    }
            //}
        }

        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver;

        RunTiaAndPlcSim();


    }

    private static TxPlcLogicBehaviorMoveToPoseAction CreateMoveToPoseAction(TxPlcLogicBehavior lb, TxPose pose, string name)
    {
        TxPlcLogicBehaviorMoveToPoseAction resultAction = null;

        var creationData = new TxPlcLogicBehaviorMoveToPoseActionCreationData() { Name = name, Pose = pose };
        // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
        resultAction = (TxPlcLogicBehaviorMoveToPoseAction)lb.CreateAction(creationData);
        return resultAction;
    }

    private static ITxPlcLogicBehaviorParameter CreateJointValueSensorParameter(TxPlcLogicBehavior lb, TxPose poses, string name, ref StringWriter output)
    {
        TxPlcLogicBehaviorAtPoseSensorData sensorData = new TxPlcLogicBehaviorAtPoseSensorData();

        // Set the target joint in the sensor data.
        sensorData.Pose = poses;

        // Depending on the joint type: default epsilon range is 2mm (prismatic joints)/ 0.035 rad = 2 degrees for revolute joints.
        double DEFAULT_EPSILON_RANGE = 2;
        //if (joint.Type == TxJoint.TxJointType.Revolute)
        // {
        //    DEFAULT_EPSILON_RANGE *= Math.PI / 180.0; // Convert from degrees to radians.
        // }

        // If don't want to specify low/high range - specify "null" or skip assigning them.
        // Pulse - both are null, Step From - high range is null / Step To - low range is null.
        // Example is for the default type (Range).
        sensorData.RangeLow = -DEFAULT_EPSILON_RANGE;
        sensorData.RangeHigh = DEFAULT_EPSILON_RANGE;
        sensorData.RangeValue = 0.0;

        // Initialize the parameter creation data.
        TxPlcLogicBehaviorAtPoseSensorParameterCreationData creationData = new TxPlcLogicBehaviorAtPoseSensorParameterCreationData();
        creationData.Name = name;
        creationData.SensorData = sensorData;

        // Create the JVS parameter
        return lb.CreateParameter(creationData);
    }

    private static TxPlcFromRobotSignal GetExistingRobotSignal(string name, TxPlcSignalDataType dataType)
    {
        var signals = TxApplication.ActiveDocument.PlcProgramRoot.CurrentPlcProgram.GetSignals(new TxTypeFilter(typeof(TxPlcFromRobotSignal)));
        foreach (TxPlcFromRobotSignal signal in signals)
        {
            if (signal.Name == name && signal.DataType == dataType)
                return signal;
        }
        return null;
    }

    private static TxPlcToRobotSignal GetExistingRobotSignalTo(string name, TxPlcSignalDataType dataType)
    {
        var signals = TxApplication.ActiveDocument.PlcProgramRoot.CurrentPlcProgram.GetSignals(new TxTypeFilter(typeof(TxPlcToRobotSignal)));
        foreach (TxPlcToRobotSignal signal in signals)
        {
            if (signal.Name == name && signal.DataType == dataType)
                return signal;
        }
        return null;
    }

    private static ITxPlcSignal GetExistingSignal(string name, TxPlcSignalDataType dataType)
    {
        var signals = TxApplication.ActiveDocument.PlcProgramRoot.CurrentPlcProgram.GetSignals(new TxTypeFilter(typeof(ITxPlcSignal)));
        foreach (ITxPlcSignal signal in signals)
        {
            if (signal.Name == name && signal.DataType == dataType)
                return signal;
        }
        return null;
    }

    private static ITxPlcSignal GetOrCreateInputSignal(string name, TxPlcSignalDataType dataType, int byteAddr, int bitaddr, string externalConnectionName)
    {
        ITxPlcSignal signal = GetExistingSignal(name, dataType);
        if (signal == null)
        {
            using (var creationData = new TxPlcInputSignalCreationData(name) { DataType = dataType })
            {
                signal = TxApplication.ActiveDocument.PlcProgramRoot.CurrentPlcProgram.CreatePlcInputSignal(creationData);
                signal.Address = new TxPlcSignalAddress(byteAddr, bitaddr);
            }
        }

        // Set external connection if provided
        if (!string.IsNullOrEmpty(externalConnectionName))
        {
            signal.ExternalConnection = externalConnectionName;
        }

        return signal;
    }
    private static ITxPlcSignal GetOrCreateOutputSignal(string name, TxPlcSignalDataType dataType, int byteAddr, int bitAdder, string externalConnectionName)
    {
        ITxPlcSignal signal = GetExistingSignal(name, dataType);
        if (signal == null)
        {
            using (var creationData = new TxPlcOutputSignalCreationData(name) { DataType = dataType })
            {
                signal = TxApplication.ActiveDocument.PlcProgramRoot.CurrentPlcProgram.CreatePlcOutputSignal(creationData);
                signal.Address = new TxPlcSignalAddress(byteAddr, bitAdder);
            }
        }

        // Set external connection if provided
        if (!string.IsNullOrEmpty(externalConnectionName))
        {
            signal.ExternalConnection = externalConnectionName;
        }

        return signal;
    }
    private static void ConnectSignalWithExternal(ITxPlcLogicBehaviorEntry entry, string name, TxPlcSignalDataType type, int addrCounter, StringWriter output)
    {
        ITxPlcSignal signal = GetOrCreateSignalWithExternal(name, type, addrCounter);
        ConnectSignalToEntry(entry, signal, output);
    }

    private static void ConnectSignalWithExternalExit(ITxPlcLogicBehaviorExit exit, string name, TxPlcSignalDataType type, ref int addrCounter, StringWriter output)
    {
        ITxPlcSignal signal = GetOrCreateSignalWithExternal(name, type, addrCounter);
        ConnectSignalToExit(exit, signal, output);
        addrCounter += 1;
    }
    private static ITxPlcSignal GetOrCreateSignalWithExternal(string name, TxPlcSignalDataType type, int address)
    {
        ITxPlcSignal signal = GetExistingSignal(name, type);
        if (signal == null)
        {
            using (var creationData = new TxPlcInputSignalCreationData(name) { DataType = type })
            {
                signal = TxApplication.ActiveDocument.PlcProgramRoot.CurrentPlcProgram.CreatePlcInputSignal(creationData);
                signal.Address = new TxPlcSignalAddress(address, 0);
            }
        }

        // Set external connection to same name
        signal.ExternalConnection = "PLC";
        return signal;
    }

    public static void ConnectSignalToExit(ITxPlcLogicBehaviorExit exit, ITxPlcSignal signal, StringWriter output)
    {
        if (exit == null || signal == null)
        {
            output.WriteLine("Exit or Signal is null. Cannot proceed.");
            return;
        }

        var connectedSignals = exit.ConnectedSignals;
        bool alreadyConnected = false;
        for (int i = 0; i < connectedSignals.Length; i++)
        {
            TxPlcSignalBase s = connectedSignals.GetValue(i) as TxPlcSignalBase;
            if (s != null && s.Name == signal.Name)
            {
                alreadyConnected = true;
                break;
            }
        }

        if (!alreadyConnected)
        {
            exit.ConnectSignal(signal as TxPlcSignalBase);
            output.WriteLine(string.Format("Signal '{0}' connected to exit '{1}'", signal.Name, exit.Name));
        }
        else
        {
            output.WriteLine(string.Format("Signal '{0}' is already connected to exit '{1}'", signal.Name, exit.Name));
        }
    }

    public static void ConnectRobotSignalToEntry(ITxPlcLogicBehaviorEntry entry, TxPlcFromRobotSignal signal, StringWriter output)
    {
        if (entry == null || signal == null)
        {
            output.WriteLine("Entry or Signal is null. Cannot proceed.");
            return;
        }

        var connectedSignals = entry.ConnectedSignals;
        bool alreadyConnected = false;
        for (int i = 0; i < connectedSignals.Length; i++)
        {
            TxPlcSignalBase s = connectedSignals.GetValue(i) as TxPlcSignalBase;
            if (s != null && s.Name == signal.Name)
            {
                alreadyConnected = true;
                break;
            }
        }

        if (!alreadyConnected)
        {
            entry.ConnectSignal(signal as TxPlcSignalBase);
            output.WriteLine(string.Format("Signal '{0}' connected to entry '{1}'", signal.Name, entry.Name));
        }
        else
        {
            output.WriteLine(string.Format("Signal '{0}' is already connected to entry '{1}'", signal.Name, entry.Name));
        }
    }

    public static void ConnectRobotSignalToExit(ITxPlcLogicBehaviorExit exit, TxPlcToRobotSignal signal, StringWriter output)
    {
        if (exit == null || signal == null)
        {
            output.WriteLine("Exit or Signal is null. Cannot proceed.");
            return;
        }

        var connectedSignals = exit.ConnectedSignals;
        bool alreadyConnected = false;
        for (int i = 0; i < connectedSignals.Length; i++)
        {
            TxPlcSignalBase s = connectedSignals.GetValue(i) as TxPlcSignalBase;
            if (s != null && s.Name == signal.Name)
            {
                alreadyConnected = true;
                break;
            }
        }

        if (!alreadyConnected)
        {
            exit.ConnectSignal(signal as TxPlcSignalBase);
            output.WriteLine(string.Format("Signal '{0}' connected to exit '{1}'", signal.Name, exit.Name));
        }
        else
        {
            output.WriteLine(string.Format("Signal '{0}' is already connected to exit '{1}'", signal.Name, exit.Name));
        }
    }

    public static void ConnectSignalToEntry(ITxPlcLogicBehaviorEntry entry, ITxPlcSignal signal, StringWriter output)
    {
        if (entry == null || signal == null)
        {
            output.WriteLine("Entry or Signal is null. Cannot proceed.");
            return;
        }

        var connectedSignals = entry.ConnectedSignals;
        bool alreadyConnected = false;
        for (int i = 0; i < connectedSignals.Length; i++)
        {
            TxPlcSignalBase s = connectedSignals.GetValue(i) as TxPlcSignalBase;
            if (s != null && s.Name == signal.Name)
            {
                alreadyConnected = true;
                break;
            }
        }

        if (!alreadyConnected)
        {
            entry.ConnectSignal(signal as TxPlcSignalBase);
            output.WriteLine(string.Format("Signal '{0}' connected to entry '{1}'", signal.Name, entry.Name));
        }
        else
        {
            output.WriteLine(string.Format("Signal '{0}' is already connected to entry '{1}'", signal.Name, entry.Name));
        }
    }
    private static ITxPlcLogicBehaviorParameter CreateJointDistanceValueParameter(TxPlcLogicBehavior lb, TxJoint joint, TxPlcHardwareType hardwareType, string name)
    {
        TxPlcLogicBehaviorJointDistanceSensorData sensorData = new TxPlcLogicBehaviorJointDistanceSensorData();

        // Set the target joint in the sensor data.
        sensorData.Joint = joint;
        sensorData.HardwareType = hardwareType;

        // Initialize the parameter creation data.
        TxPlcLogicBehaviorJointDistanceSensorParameterCreationData creationData = new TxPlcLogicBehaviorJointDistanceSensorParameterCreationData();
        creationData.Name = name;// "JDS_" + joint.Name;
        creationData.SensorData = sensorData;

        // Create the JVS parameter
        return lb.CreateParameter(creationData);
    }

    private static ITxPlcLogicBehaviorParameter CreateJointDistanceValueParameterUpd(TxPlcLogicBehavior lb, TxJoint joint, TxPlcHardwareType hardwareType, string name, ref StringWriter output)
    {
        TxPlcLogicBehaviorJointDistanceSensorData sensorData = new TxPlcLogicBehaviorJointDistanceSensorData();
        TxPlcLogicBehaviorJointDistanceSensorParameterCreationData creationData = new TxPlcLogicBehaviorJointDistanceSensorParameterCreationData();

        try
        {
            //TxPlcLogicBehaviorJointDistanceSensorData sensorData = new TxPlcLogicBehaviorJointDistanceSensorData();

            // Set the target joint in the sensor data.
            sensorData.Joint = joint;
            sensorData.HardwareType = hardwareType;

            // Initialize the parameter creation data.
            //TxPlcLogicBehaviorJointDistanceSensorParameterCreationData creationData = new TxPlcLogicBehaviorJointDistanceSensorParameterCreationData();
            creationData.Name = name;
            creationData.SensorData = sensorData;

            // Create the JVS parameter
            return lb.CreateParameter(creationData);
        }
        catch (Exception exx2)
        {
            output.WriteLine("Error For Create Joint Distance Sensor: " + exx2.Message);
        }
        return lb.CreateParameter(creationData);
    }


    // Example how to create JVS parameter for joint
    private static ITxPlcLogicBehaviorParameter CreateJointValueSensorParameter(TxPlcLogicBehavior lb, TxJoint joint)
    {
        TxPlcLogicBehaviorAtPoseSensorData sensorData = new TxPlcLogicBehaviorAtPoseSensorData();

        // Set the target joint in the sensor data.
        sensorData.Joint = joint;

        // Depending on the joint type: default epsilon range is 2mm (prismatic joints)/ 0.035 rad = 2 degrees for revolute joints.
        double DEFAULT_EPSILON_RANGE = 2;
        if (joint.Type == TxJoint.TxJointType.Revolute)
        {
            DEFAULT_EPSILON_RANGE *= Math.PI / 180.0; // Convert from degrees to radians.
        }

        // If don't want to specify low/high range - specify "null" or skip assigning them. 
        // Pulse - both are null, Step From - high range is null / Step To - low range is null.
        // Example is for the default type (Range).
        sensorData.RangeLow = -DEFAULT_EPSILON_RANGE;
        sensorData.RangeHigh = DEFAULT_EPSILON_RANGE;
        sensorData.RangeValue = 0.0;

        // Initialize the parameter creation data.
        TxPlcLogicBehaviorAtPoseSensorParameterCreationData creationData = new TxPlcLogicBehaviorAtPoseSensorParameterCreationData();
        creationData.Name = "JVS_" + joint.Name;
        creationData.SensorData = sensorData;

        // Create the JVS parameter
        return lb.CreateParameter(creationData);
    }


    private static ITxPlcLogicBehaviorParameter CreateJointDistanceSensorParameter(TxPlcLogicBehavior lb, TxJoint joint)
    {
        TxPlcLogicBehaviorJointDistanceSensorData sensorData = new TxPlcLogicBehaviorJointDistanceSensorData();

        // Set the target joint in the sensor data.
        sensorData.HardwareType = TxPlcHardwareType.Real;

        // Depending on the joint type: default epsilon range is 2mm (prismatic joints)/ 0.035 rad = 2 degrees for revolute joints.
        ;

        // Initialize the parameter creation data.
        TxPlcLogicBehaviorJointDistanceSensorParameterCreationData creationData = new TxPlcLogicBehaviorJointDistanceSensorParameterCreationData();
        creationData.Name = "Demo1";
        creationData.SensorData = sensorData;

        // Create the JVS parameter
        return lb.CreateParameter(creationData);
    }


    // Example how to create JVS parameter for pose.
    private static ITxPlcLogicBehaviorParameter CreatePoseSensorParameter(TxPlcLogicBehavior lb, TxPose pose)
    {
        TxPlcLogicBehaviorAtPoseSensorData sensorData = new TxPlcLogicBehaviorAtPoseSensorData();

        // Set the target pose in the sensor data.
        sensorData.Pose = pose;

        // For the range consider the joint type of the 1st driving joint.
        var joint = (lb.Resource as ITxDevice).DrivingJoints[0] as TxJoint;

        // Depending on the joint type: default epsilon range is 2mm (prismatic joints)/ 0.035 rad = 2 degrees for revolute joints.
        double DEFAULT_EPSILON_RANGE = 2;
        if (joint.Type == TxJoint.TxJointType.Revolute)
        {
            DEFAULT_EPSILON_RANGE *= Math.PI / 180.0; // Convert from degrees to radians.
        }

        // If don't want to specify low/high range - specify "null" or skip assigning them. 
        // Pulse - both are null, Step From - high range is null / Step To - low range is null.
        // Example is for the default type (Range).
        sensorData.RangeLow = -DEFAULT_EPSILON_RANGE;
        sensorData.RangeHigh = DEFAULT_EPSILON_RANGE;
        sensorData.RangeValue = 0.0;

        // Initialize the parameter creation data.
        TxPlcLogicBehaviorAtPoseSensorParameterCreationData creationData = new TxPlcLogicBehaviorAtPoseSensorParameterCreationData();
        creationData.Name = "PS_" + pose.Name;
        creationData.SensorData = sensorData;

        // Create the JVS parameter
        return lb.CreateParameter(creationData);
    }

    private static ITxPlcLogicBehaviorAction CreateGripAction(TxPlcLogicBehavior lb, string name)
    {
        ITxPlcLogicBehaviorAction resultAction = null;

        // It's mandatory to specify a frame (to which the objects will be attached).
        // In the example will be used the gripper TCPF
        TxFrame applyToFrame = (lb.Resource as ITxTool).TCPF;
        if (applyToFrame == null)
        {
            // Alternatively can be used any frames which appear under the gripper (these frames have to be set as preserved if done end-modelling).
            var frames = (lb.Resource as ITxObjectCollection).GetAllDescendants(new TxTypeFilter(typeof(TxFrame)));
            if (frames.Count > 0)
            {
                applyToFrame = frames[0] as TxFrame;
            }
        }

        var creationData = new TxPlcLogicBehaviorGripActionCreationData() { Name = name, frame = applyToFrame };
        // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
        resultAction = lb.CreateAction(creationData);

        return resultAction;
    }

    private static ITxPlcLogicBehaviorAction CreateGripActionUpd(TxPlcLogicBehavior lb, string name, string TCP, ref StringWriter output)
    {
        ITxPlcLogicBehaviorAction resultAction = null;

        // It's mandatory to specify a frame (to which the objects will be attached).
        // In the example will be used the gripper TCPF

        //TxObjectList poses = (lb.Resource as ITxDevice).PoseList;
        //TxPose pose1 = poses[0] as TxPose;

        TxFrame applyToFrame = (lb.Resource as ITxTool).TCPF;
        //if (applyToFrame == null)
        //{
        // Alternatively can be used any frames which appear under the gripper (these frames have to be set as preserved if done end-modelling).
        var frames = (lb.Resource as ITxObjectCollection).GetAllDescendants(new TxTypeFilter(typeof(TxFrame)));
        if (frames.Count > 0)
        {
            if (TCP == "TCP1")
            {
                //output.WriteLine("TCP1 Found");
                applyToFrame = frames[0] as TxFrame;
            }
            else if (TCP == "TCP2")
            {
                //output.WriteLine("TCP2 Found");
                applyToFrame = frames[1] as TxFrame;
            }
            else if (TCP == "TCP3")
            {
                //output.WriteLine("TCP3 Found");
                applyToFrame = frames[2] as TxFrame;
            }
            //output.WriteLine("Not Found");
            //else if (TCP == "Grip1")
            //    pose1 = poses[0] as TxPose;
            //else if (TCP == "Grip2")
            //    pose1 = poses[1] as TxPose;
            //else if (TCP == "Grip3")
            //    pose1 = poses[2] as TxPose;
        }
        else
        {
            //output.WriteLine("TCP Count Is 0");
        }
        //}
        //else
        //{
        //    output.WriteLine("Frame Not null");
        //}
        //output.WriteLine("Name of Frame : " + applyToFrame.Name);
        var creationData = new TxPlcLogicBehaviorGripActionCreationData() { Name = name, frame = applyToFrame };
        // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
        resultAction = lb.CreateAction(creationData);

        return resultAction;
    }

    private static TxPlcExpression CreateSignedValueExpression(int number)
    {
        TxPlcExpressionBuilder builder = new TxPlcExpressionBuilder();

        if (number < 0)
        {
            // Negative ? wrap in quotes "-40"
            string strVal = number.ToString();
            builder.Add(strVal);
        }
        else
        {
            // Positive ? add as DInt
            TxPlcValue val = new TxPlcValue();
            val.DIntValue = number;
            builder.Add(TxPlcSignalDataType.DInt, val);
        }

        return builder.Expression;
    }

    private static TxPlcLogicBehaviorMoveJointAction CreateMoveJointActionUpd(TxPlcLogicBehavior lb, TxJoint joint, string name)
    {
        TxPlcLogicBehaviorMoveJointAction resultAction = null;

        var creationData = new TxPlcLogicBehaviorMoveJointActionCreationData() { Name = name, Joint = joint };
        // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
        resultAction = (TxPlcLogicBehaviorMoveJointAction)lb.CreateAction(creationData);
        return resultAction;
    }

    private static ITxPlcLogicBehaviorConstant CreateConstant(TxPlcLogicBehavior lb, string name, TxPlcHardwareType hardwareType, TxPlcValue value)
    {

        foreach (ITxPlcLogicBehaviorConstant c in lb.Constants)
            if (c.Name == name) return c;

        var creationData = new TxPlcLogicBehaviorConstantCreationData()
        {
            Name = name,
            HardwareType = hardwareType,
            Value = value
        };

        return lb.CreateConstant(creationData);
    }

    private static ITxPlcLogicBehaviorAction CreateReleaseActionUpd(TxPlcLogicBehavior lb, string name, string TCP)
    {
        ITxPlcLogicBehaviorAction resultAction = null;

        // It's mandatory to specify a frame (to which the objects will be attached).
        // In the example will be used the gripper TCPF

        //TxObjectList poses = (lb.Resource as ITxDevice).PoseList;
        //TxPose pose1 = poses[0] as TxPose;

        TxFrame applyToFrame = (lb.Resource as ITxTool).TCPF;
        //if (applyToFrame == null)
        //{
        // Alternatively can be used any frames which appear under the gripper (these frames have to be set as preserved if done end-modelling).
        var frames = (lb.Resource as ITxObjectCollection).GetAllDescendants(new TxTypeFilter(typeof(TxFrame)));
        if (frames.Count > 0)
        {
            if (TCP == "TCP1")
            {
                applyToFrame = frames[0] as TxFrame;
            }
            else if (TCP == "TCP2")
            {
                applyToFrame = frames[1] as TxFrame;
            }
            else if (TCP == "TCP3")
            {
                applyToFrame = frames[2] as TxFrame;
            }
            //else if (TCP == "Grip1")
            //    pose1 = poses[0] as TxPose;
            //else if (TCP == "Grip2")
            //    pose1 = poses[1] as TxPose;
            //else if (TCP == "Grip3")
            //    pose1 = poses[2] as TxPose;
        }
        // }

        var creationData = new TxPlcLogicBehaviorReleaseActionCreationData() { Name = name, frame = applyToFrame };
        // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
        resultAction = lb.CreateAction(creationData);

        return resultAction;
    }

    // Get a logic resource by name (V1 - get all the filtered logic resource & from these return the first wit.h the provided name).
    private static ITxPlcLogicResource GetLogicResourceByName_V1(string name)
    {
        ITxPlcLogicResource result = null;
        var logicResources = TxApplication.ActiveDocument.PhysicalRoot.GetAllDescendants(new TxTypeFilter(typeof(ITxPlcLogicResource)));
        foreach (ITxPlcLogicResource logicResource in logicResources)
        {
            if (logicResource.Name == name)
            {
                result = logicResource;
                break;
            }
        }

        return result;
    }


    // Example how to create 'MOVE JOINT TO VALUE' action
    private static TxPlcLogicBehaviorMoveJointToValueAction CreateMoveJointToValueAction(TxPlcLogicBehavior lb, TxJoint joint, string name)
    {
        TxPlcLogicBehaviorMoveJointToValueAction resultAction = null;

        var creationData = new TxPlcLogicBehaviorMoveJointToValueActionCreationData() { Name = name, Joint = joint };
        // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
        resultAction = (TxPlcLogicBehaviorMoveJointToValueAction)lb.CreateAction(creationData);
        return resultAction;
    }

    // Example how to create 'MOVE JOINT' action

    // Example how to create 'JUMP JOINT' action
    private static TxPlcLogicBehaviorJumpJointAction CreateJumpJointAction(TxPlcLogicBehavior lb, TxJoint joint, string name)
    {
        TxPlcLogicBehaviorJumpJointAction resultAction = null;

        var creationData = new TxPlcLogicBehaviorJumpJointActionCreationData() { Name = name, Joint = joint };
        // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
        resultAction = (TxPlcLogicBehaviorJumpJointAction)lb.CreateAction(creationData);
        return resultAction;
    }

    // Example how to create 'JOINT VELOCITY CONTROLLED' action
    private static TxPlcLogicBehaviorJointVelocityControlledAction CreateJointVelocityControlledAction(TxPlcLogicBehavior lb, TxJoint joint, string name)
    {
        TxPlcLogicBehaviorJointVelocityControlledAction resultAction = null;

        var creationData = new TxPlcLogicBehaviorJointVelocityControlledActionCreationData() { Name = name, Joint = joint };
        // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
        resultAction = (TxPlcLogicBehaviorJointVelocityControlledAction)lb.CreateAction(creationData);
        return resultAction;
    }

    // Example how to create 'JOINT ACCELERATION CONTROLLED' action
    private static TxPlcLogicBehaviorJointAccelerationControlledAction CreateJointAccelerationControlledAction(TxPlcLogicBehavior lb, TxJoint joint, string name)
    {
        TxPlcLogicBehaviorJointAccelerationControlledAction resultAction = null;

        var creationData = new TxPlcLogicBehaviorJointAccelerationControlledActionCreationData() { Name = name, Joint = joint };
        // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
        resultAction = (TxPlcLogicBehaviorJointAccelerationControlledAction)lb.CreateAction(creationData);
        return resultAction;
    }

    // Example how to create 'MOVE TO POSE' action
    // private static TxPlcLogicBehaviorMoveToPoseAction CreateMoveToPoseAction(TxPlcLogicBehavior lb, TxPose pose, string name)
    // {
    //     TxPlcLogicBehaviorMoveToPoseAction resultAction = null;

    //     var creationData = new TxPlcLogicBehaviorMoveToPoseActionCreationData() { Name = name, Pose = pose };
    // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
    //      resultAction = (TxPlcLogicBehaviorMoveToPoseAction)lb.CreateAction(creationData);
    //     return resultAction;
    // }


    // Example how to create RELEASE action
    private static ITxPlcLogicBehaviorAction CreateReleaseAction(TxPlcLogicBehavior lb, string name)
    {
        ITxPlcLogicBehaviorAction resultAction = null;

        // It's mandatory to specify a frame (to which the objects will be attached).
        // In the example will be used the gripper TCPF
        TxFrame applyToFrame = (lb.Resource as ITxTool).TCPF;
        if (applyToFrame == null)
        {
            // Alternatively can be used any frames which appear under the gripper (these frames have to be set as preserved if done end-modelling).
            var frames = (lb.Resource as ITxObjectCollection).GetAllDescendants(new TxTypeFilter(typeof(TxFrame)));
            if (frames.Count > 0)
            {
                applyToFrame = frames[0] as TxFrame;
            }
        }

        var creationData = new TxPlcLogicBehaviorReleaseActionCreationData() { Name = name, frame = applyToFrame };
        // Note it is recommended to use also lb.CanCreateElement(creationData) to check if creation is allowed.
        resultAction = lb.CreateAction(creationData);

        return resultAction;
    }

    // Get a logic resource by name (V1 - get all the objects with the given name & from these return the first which is a logic resource).
    private static ITxPlcLogicResource GetLogicResourceByName_V2(string name)
    {
        ITxPlcLogicResource result = null;
        var objectsByName = TxApplication.ActiveDocument.GetObjectsByName(name);
        foreach (ITxObject obj in objectsByName)
        {
            ITxPlcLogicResource logicResource = obj as ITxPlcLogicResource;
            if (logicResource != null)
            {
                result = logicResource;
                break;
            }
        }

        return result;
    }

    // Create a new logic resource (method is relevant for eMS/eMS Disc platforms).
    private static ITxPlcLogicResource CreateLogicResource_eMS(ITxObjectCollection parent, string name)
    {
        using (var resourceCreationData = new TxResourceCreationData(name) { PmType = "PmToolPrototype" })
        {
            var resource = TxApplication.ActiveDocument.PhysicalRoot.CreateResource(resourceCreationData) as ITxPlcLogicResource;
            if (parent is ITxComponent && (parent as ITxComponent).IsEquipment && (parent as ITxComponent).IsOpenForModeling)
            {
                // Note: Adding under equipment is a special case - AddObject copies the resource under the equipment + deletes the original.
                return MoveResourceUnderEquipment(parent as ITxComponent, resource as ITxComponent) as ITxPlcLogicResource;
            }
            else if (parent is TxCompoundResource)// Add the resource under the compound resource.
            {
                parent.AddObject(resource);
            }
            else if (parent is ITxFolder)// Add the resource under the folder (only if it's child folder within 'Resources' folder hierarchy)
            {
                if (IsResourcesChildFolder(parent as ITxFolder))
                {
                    parent.AddObject(resource);
                }
            }

            return resource;
        }
    }

    //private static TxPlcLogicBehaviorExpressionValueParameter CreateParameterExp(TxPlcLogicBehavior lb, string name, TxPlcHardwareType hardwareType)
    //{
    //    // If already existing entry with this name - return it (diregarding the hardware type).
    //    foreach (TxPlcLogicBehaviorExpressionValueParameter parameter in lb.Parameters)
    //    {
    //        if (parameter.Name == name)
    //        {
    //            return parameter;
    //        }
    //    }

    //    return lb.CreateParameter(new TxPlcLogicBehaviorExpressionValueParameterCreationData() { Name = name, HardwareType = hardwareType });
    //}

    private static ITxPlcLogicBehaviorParameter CreateParameter(TxPlcLogicBehavior lb, string name, TxPlcHardwareType hardwareType)
    {
        // If already existing entry with this name - return it (diregarding the hardware type).
        foreach (ITxPlcLogicBehaviorParameter parameter in lb.Parameters)
        {
            if (parameter.Name == name)
            {
                return parameter;
            }
        }

        return lb.CreateParameter(new TxPlcLogicBehaviorExpressionValueParameterCreationData() { Name = name, HardwareType = hardwareType });
    }

    /// <summary>
    /// Move a component under parent equipment and return the result inserted component (a new one).
    /// </summary>
    /// <param name="parentEquipment">The parent equipment.</param>
    /// <param name="component">The original component to be moved.</param>
    /// <returns>The new component under the equipment.</returns>
    /// <remarks>Special handling required due to internally original component is copied and afterwards deleted.</remarks>
    private static ITxComponent MoveResourceUnderEquipment(ITxComponent parentEquipment, ITxComponent component)
    {
        var parentColl = parentEquipment as ITxObjectCollection;

        // Consider which is the inserted component based on old/new direct component children.
        var oldDirectChildComponents = parentColl.GetDirectDescendants(new TxTypeFilter(typeof(ITxComponent)));
        parentColl.AddObject(component);
        var newDirectChildComponents = parentColl.GetDirectDescendants(new TxTypeFilter(typeof(ITxComponent)));

        // If we have difference between old & new direct component children -> return the new child.
        if (newDirectChildComponents.Count != oldDirectChildComponents.Count)
        {
            foreach (var newDirectChild in newDirectChildComponents)
            {
                if (!oldDirectChildComponents.Contains(newDirectChild))
                {
                    return newDirectChild as ITxComponent;
                }
            }
        }

        // In case of add failiure - return the original component.
        return component;
    }

    // Gets 'Resources' main folder
    private static ITxMainFolder GetResourcesMainFolder()
    {
        var mainFolders = TxApplication.ActiveDocument.MainFolders;
        foreach (var mainFolder in mainFolders)
        {
            if (mainFolder.FolderType == TxMainFolderType.Resources)
            {
                return mainFolder;
            }
        }
        return null;
    }

    // Returns whether the folder is child of the 'Resources' main folder.
    private static bool IsResourcesChildFolder(ITxFolder folder)
    {
        var resourcesFolder = GetResourcesMainFolder();
        if (resourcesFolder != null && folder != resourcesFolder && IsChildFolder(resourcesFolder, folder))
        {
            return true;
        }

        return false;
    }

    // Returns whether the folder is child of the given parent folder.
    private static bool IsChildFolder(ITxFolder parentFolder, ITxFolder childFolder)
    {
        var allChildren = parentFolder.GetAllDescendants(new TxTypeFilter(typeof(ITxFolder)));
        return allChildren.Contains(childFolder);
    }

    private static string ShowSelectionDialog(List<string> items)
    {
        string selectedItem = null;

        using (Form form = new Form())
        {
            form.Text = "Select Logic Resource";
            form.Width = 400;
            form.Height = 200;

            // Main ListBox                                         
            var listBox = new ListBox() { Dock = DockStyle.Fill };
            listBox.Items.AddRange(items.ToArray());

            // OK button
            var okButton = new Button() { Text = "OK", Dock = DockStyle.Bottom };
            okButton.Click += (sender, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    selectedItem = listBox.SelectedItem.ToString();
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                }
                else
                {
                    MessageBox.Show("Please select a resource.");
                }
            };

            //// Create New LB button ? directly return "New"
            //var createNewButton = new Button() { Text = "Create New LB", Dock = DockStyle.Bottom };
            //createNewButton.Click += (sender, e) =>
            //{
            //    selectedItem = "New";  // return "New" when clicked
            //    form.DialogResult = DialogResult.OK;
            //    form.Close();
            //};

            form.Controls.Add(listBox);
            //form.Controls.Add(createNewButton);
            form.Controls.Add(okButton);

            form.ShowDialog();
        }

        return selectedItem;
    }


    #region Private Methods

    /// <summary>
    /// Example 1 - Creates a simple value expression with "TRUE" value.
    /// </summary>
    /// <returns>The created expression.</returns>
    private static TxPlcExpression CreateExpression_TRUE()
    {
        TxPlcExpressionBuilder expressionBuilder = new TxPlcExpressionBuilder();

        // Hardware type identifies how the PLC value has to be converted to text.
        TxPlcSignalDataType hardwareType = TxPlcSignalDataType.Bool;

        // Creates a new value & initialize it as TRUE.
        TxPlcValue value = new TxPlcValue();
        value.BooleanValue = true;

        // Add the value operand.
        expressionBuilder.Add(hardwareType, value);

        // Create the expression.
        // Note: expressionBuilder.Expression needs to be called once all operands are already added.
        // It's not a regular getter, but a build action (in case of error like adding operands/operators in wrong order - null expression will be returned).
        return expressionBuilder.Expression;
    }

    /// <summary>
    /// Example 2 - Creates an expression = A + B.
    /// </summary>
    /// <param name="A">The entry "A".</param>
    /// <param name="B">The entry "B".</param>
    /// <returns></returns>
    private static TxPlcExpression CreateExpression_A_PLUS_B(ITxPlcLogicBehaviorEntry A, ITxPlcLogicBehaviorEntry B)
    {
        TxPlcExpressionBuilder expressionBuilder = new TxPlcExpressionBuilder();

        // Add the "A" operand.
        expressionBuilder.Add(A);
        // Add the "+" operator.
        expressionBuilder.Add(TxPlcExpressionOperator.Plus);
        // Add the "B" operand.
        expressionBuilder.Add(B);

        // Create the expression.
        return expressionBuilder.Expression;
    }


    private static TxPlcExpression CreateExpression(string Exp, TxPlcLogicBehavior lb, ref StringWriter output)
    {
        TxPlcExpressionBuilder expressionBuilder = new TxPlcExpressionBuilder();
        string[] words = Exp.Split(' ');

        try
        {
            if (Exp == "BinSensor")
            {
                //output.WriteLine("");
                TxPlcFunctionParameterData functionParam = new TxPlcFunctionParameterData();

                string name = "BinSensor";
                //foreach (var v1 in words)
                //{
                //    if (v1.Contains("\""))
                //        name = v1;

                //    else
                //        name = "Demo";
                //}
                ITxPlcLogicBehaviorEntry tag = null;
                foreach (ITxPlcLogicBehaviorEntry entry in lb.Entries)
                {
                    //output.WriteLine(name);
                    if (entry.Name == name)
                    {
                        tag = entry;
                    }
                }
                if (tag == null)
                {
                    //output.WriteLine("null");
                }
                else
                {
                    //output.WriteLine(tag.Name);
                }

                //functionParam.SetAsValue(TxPlcSignalDataType.Bool, tag as TxPlcValue);

                //functionParam.SetAs(tag);
                ArrayList functionParameters = new ArrayList() { tag };


                //expressionBuilder.Add(TxPlcExpressionFunctionType.RE, functionParameters);
                expressionBuilder.Add(tag);
                //expressionBuilder.TryAdd(tag as ITxPlcSignal);
                return expressionBuilder.Expression;
            }
        }
        catch (Exception exx6)
        {
            output.WriteLine("Error For Create Expression: " + exx6.Message);
        }

        try
        {
            if (Exp.Contains("RE"))
            {
                //int start = Exp.IndexOf("\"\"") + 2; // skip two quotes
                //int end = Exp.LastIndexOf("\"\"");

                //string result = Exp.Substring(start, end - start);

                int start = Exp.IndexOf("(") + 1;
                int end = Exp.IndexOf(")");
                string result = Exp.Substring(start, end - start);

                //output.WriteLine(result);

                //output.WriteLine("");
                TxPlcFunctionParameterData functionParam = new TxPlcFunctionParameterData();

                string name = result;// "FingerSensor";

                ITxPlcLogicBehaviorEntry tag = null;
                foreach (ITxPlcLogicBehaviorEntry entry in lb.Entries)
                {
                    //output.WriteLine(name);
                    if (entry.Name == name)
                    {
                        tag = entry;
                    }
                }
                if (tag == null)
                {
                    //output.WriteLine("null");
                }
                else
                {
                    //output.WriteLine(tag.Name);
                }

                //functionParam.SetAsValue(TxPlcSignalDataType.Bool, tag as TxPlcValue);
                functionParam.SetAsEntry(tag);
                //functionParam.SetAs(tag);

                ArrayList functionParameters = new ArrayList() { functionParam };
                //output.WriteLine("6");

                //expressionBuilder.Add(TxPlcExpressionFunctionType.RE, functionParameters);
                expressionBuilder.Add(TxPlcExpressionFunctionType.RE, functionParameters);
                //expressionBuilder.TryAdd(tag as ITxPlcSignal);
                //output.WriteLine("7");
                return expressionBuilder.Expression;
            }
        }
        catch (Exception e1)
        {
            output.WriteLine("Error For Create Expression: " + e1.Message);
        }
        if (Exp.Contains("FE"))
        {
            //int start = Exp.IndexOf("\"\"") + 2; // skip two quotes
            //int end = Exp.LastIndexOf("\"\"");

            //string result = Exp.Substring(start, end - start);

            int start = Exp.IndexOf("(") + 1;
            int end = Exp.IndexOf(")");
            string result = Exp.Substring(start, end - start);

            //output.WriteLine(result);

            //output.WriteLine("");
            TxPlcFunctionParameterData functionParam = new TxPlcFunctionParameterData();
            //output.WriteLine("2");
            string name = result;// "FingerSensor";
            //foreach (var v1 in words)
            //{
            //    if (v1.Contains("\""))
            //        name = v1;

            //    else
            //        name = "Demo";
            //}
            //output.WriteLine("3");
            ITxPlcLogicBehaviorEntry tag = null;
            foreach (ITxPlcLogicBehaviorEntry entry in lb.Entries)
            {
                //output.WriteLine(name);
                if (entry.Name == name)
                {
                    tag = entry;
                }
            }
            if (tag == null)
            {
                //output.WriteLine("null");
            }
            else
            {
                //output.WriteLine(tag.Name);
            }


            //output.WriteLine("4");
            //functionParam.SetAsValue(TxPlcSignalDataType.Bool, tag as TxPlcValue);
            functionParam.SetAsEntry(tag);
            //functionParam.SetAs(tag);
            //output.WriteLine("5");
            ArrayList functionParameters = new ArrayList() { functionParam };
            //output.WriteLine("6");

            //expressionBuilder.Add(TxPlcExpressionFunctionType.RE, functionParameters);
            expressionBuilder.Add(TxPlcExpressionFunctionType.FE, functionParameters);
            //expressionBuilder.TryAdd(tag as ITxPlcSignal);
            //output.WriteLine("7");
            return expressionBuilder.Expression;
        }
        //if (!Exp.Contains("RE") || !Exp.Contains("FE") || Exp != "BinSensor")
        //{
        //    if (Exp.Contains("-"))
        //        Exp = Exp.Replace("-"," ");

        //    expressionBuilder.Add(Exp);
        //}
        try
        {

            if (!Exp.Contains("RE") || !Exp.Contains("FE") || Exp != "BinSensor")
            {
                TxPlcFunctionParameterData functionParam = new TxPlcFunctionParameterData();
                //List<string> Data = new List<string> (){"AND","OR","NOT","XOR","+","-","/","*","<",">","=","(",")" }
                List<string> operators = new List<string>
                {
                    "AND","OR","NOT","XOR","+","-","/","*","<",">","=","(",")"
                };

                List<string> tokens = new List<string>();
                string current = "";

                for (int i = 0; i < Exp.Length; i++)
                {
                    string ch = Exp[i].ToString();

                    if (char.IsWhiteSpace(Exp[i]))
                    {
                        if (!string.IsNullOrEmpty(current))
                        {
                            tokens.Add(current);
                            current = "";
                        }
                        continue;
                    }

                    if (operators.Contains(ch))
                    {
                        if (!string.IsNullOrEmpty(current))
                        {
                            tokens.Add(current);
                            current = "";
                        }
                        tokens.Add(ch);
                        continue;
                    }

                    if (char.IsLetter(Exp[i]))
                    {
                        current += Exp[i];

                        if (i == Exp.Length - 1 || !char.IsLetter(Exp[i + 1]))
                        {
                            if (operators.Contains(current))
                            {
                                tokens.Add(current);
                                current = "";
                            }
                        }
                        continue;
                    }
                    current += Exp[i];
                }

                if (!string.IsNullOrEmpty(current))
                    tokens.Add(current);

                if (tokens.Count > 0)
                {
                    foreach (string part in tokens)
                    {
                        string partname = part;
                        if (partname.Contains("\""))
                        {
                            partname = partname.Replace("\"", "");
                        }
                        string parts = part.Trim();

                        short number = 0;

                        if (short.TryParse(parts, out number))
                        {
                            //output.WriteLine("Integer: " + number);
                            TxPlcValue v = new TxPlcValue { IntValue = number };
                            expressionBuilder.Add(TxPlcSignalDataType.Int, v);
                            continue;
                        }


                        //output.WriteLine(parts);

                        switch (parts)
                        {
                            case "AND":
                                expressionBuilder.Add(TxPlcExpressionOperator.And);
                                //output.WriteLine("Found logical AND");
                                break;

                            case "OR":
                                expressionBuilder.Add(TxPlcExpressionOperator.Or);
                                //output.WriteLine("Found logical OR");
                                break;

                            case "NOT":
                                expressionBuilder.Add(TxPlcExpressionOperator.Not);
                                //output.WriteLine("Found logical NOT");
                                break;

                            case "XOR":
                                expressionBuilder.Add(TxPlcExpressionOperator.Xor);
                                //output.WriteLine("Found logical XOR");
                                break;

                            case "+":
                                expressionBuilder.Add(TxPlcExpressionOperator.Plus);
                                //output.WriteLine("Found addition");
                                break;

                            case "-":
                                expressionBuilder.Add(TxPlcExpressionOperator.Minus);
                                //output.WriteLine("Found subtraction");
                                break;

                            case "*":
                                expressionBuilder.Add(TxPlcExpressionOperator.Multiply);
                                //output.WriteLine("Found multiplication");
                                break;

                            case "/":
                                expressionBuilder.Add(TxPlcExpressionOperator.Division);
                                //output.WriteLine("Found division");
                                break;

                            case "<":
                                expressionBuilder.Add(TxPlcExpressionOperator.IsSmallerThan);
                                //output.WriteLine("Found less than");
                                break;

                            case ">":
                                expressionBuilder.Add(TxPlcExpressionOperator.IsBiggerThan);
                                //output.WriteLine("Found greater than");
                                break;

                            case "=":
                                expressionBuilder.Add(TxPlcExpressionOperator.IsEqualTo);
                                //output.WriteLine("Found equals");
                                break;

                            case "(":
                                expressionBuilder.Add(TxPlcExpressionOperator.LeftParenthesis);
                                //output.WriteLine("Found open parenthesis");
                                break;

                            case ")":
                                expressionBuilder.Add(TxPlcExpressionOperator.RightParenthesis);
                                //output.WriteLine("Found close parenthesis");
                                break;

                            default:

                                ITxPlcLogicBehaviorEntry tag = null;
                                //if (number != 0)
                                //{
                                //    TxPlcValue v1 = new TxPlcValue();
                                //    v1.IntValue = number;
                                //    expressionBuilder.Add(TxPlcSignalDataType.Int, v1);
                                //    break;
                                //}
                                foreach (ITxPlcLogicBehaviorEntry entry in lb.Entries)
                                {
                                    string name = parts.Trim();
                                    if (entry.Name == name)
                                    {
                                        tag = entry;
                                        //output.WriteLine("Entry found" + name);
                                        expressionBuilder.Add(tag);
                                        break;
                                    }
                                }

                                ITxPlcLogicBehaviorParameter parameter = null;
                                if (tag == null)
                                {
                                    foreach (ITxPlcLogicBehaviorParameter para in lb.Parameters)
                                    {
                                        //output.WriteLine("Parameter Name ==" + para.Name);
                                        string name = parts.Trim();
                                        if (para.Name == name)
                                        {
                                            //output.WriteLine("Parameter found" + name);
                                            parameter = para;
                                            expressionBuilder.Add(para);
                                            break;
                                        }
                                    }
                                }

                                ITxPlcLogicBehaviorConstant constant = null;
                                if (tag == null && parameter == null)
                                {
                                    foreach (ITxPlcLogicBehaviorConstant consta in lb.Constants)
                                    {
                                        string name = parts.Trim();
                                        if (consta.Name == name)
                                        {
                                            //output.WriteLine("const found" + name);
                                            constant = consta;
                                            expressionBuilder.Add(constant);
                                            break;
                                        }
                                    }
                                }

                                if (tag == null && parameter == null && constant == null)
                                {
                                    string name = parts.Trim();
                                    expressionBuilder.Add(name);
                                }
                                //Console.WriteLine($"Operand/Identifier: {part}");
                                break;
                        }
                    }
                }
            }
        }
        catch (Exception ess1)
        {
            output.WriteLine("Error For Create Expression: " + ess1.Message);
        }



        //foreach (var w1 in words)
        //{

        //else if (w1.trim().Contains("("))
        //{
        //    expressionBuilder.Add(TxPlcExpressionOperator.LeftParenthesis);
        //}
        //else if (w1.trim().Contains(")"))
        //{
        //    expressionBuilder.Add(TxPlcExpressionOperator.RightParenthesis);
        //}
        //else if (w1.trim().Contains("RE"))
        //{
        //    TxPlcFunctionParameterData functionParam = new TxPlcFunctionParameterData();

        //    string name = "FingerSensor";
        //    ITxPlcLogicBehaviorEntry tag = null;
        //    foreach (ITxPlcLogicBehaviorEntry entry in lb.Entries)
        //    {

        //        if (entry.Name == name)
        //        {
        //            tag = entry;
        //        }
        //    }

        //    functionParam.SetAsValue(TxPlcSignalDataType.Bool, tag);

        //    ArrayList functionParameters = new ArrayList() { functionParam };

        //    expressionBuilder.Add(TxPlcExpressionFunctionType.RE, functionParameters);
        //}
        //expressionBuilder;
        //}


        ////// Hardware type identifies how the PLC value has to be converted to text.
        ////TxPlcSignalDataType hardwareType = TxPlcSignalDataType.Bool;

        ////// Creates a new value & initialize it as TRUE.
        ////TxPlcValue value = new TxPlcValue();
        ////value.BooleanValue = true;

        ////// Add the value operand.
        ////expressionBuilder.Add(hardwareType, value);

        // Create the expression.
        // Note: expressionBuilder.Expression needs to be called once all operands are already added.
        // It's not a regular getter, but a build action (in case of error like adding operands/operators in wrong order - null expression will be returned).
        return expressionBuilder.Expression;
    }

    /// <summary>
    /// Example 3 - Creates an expression demonstrating use of parenthesis = (A - B) * multiplier.
    /// </summary>
    /// <param name="A">The entry "A".</param>
    /// <param name="B">The entry "B".</param>
    /// <param name="multiplier">The multiplier.</param>
    /// <returns>The created expression.</returns>
    private static TxPlcExpression CreateExpression_USING_PARENTHESIS(ITxPlcLogicBehaviorEntry A, ITxPlcLogicBehaviorEntry B, double multiplier)
    {
        TxPlcExpressionBuilder expressionBuilder = new TxPlcExpressionBuilder();

        // Add the left parenthesis.
        expressionBuilder.Add(TxPlcExpressionOperator.LeftParenthesis);
        // Add the "A" operand.
        expressionBuilder.Add(A);
        // Add the "-" operator.
        expressionBuilder.Add(TxPlcExpressionOperator.Minus);
        // Add the "B" operand.
        expressionBuilder.Add(B);
        // Add the right parenthesis.
        expressionBuilder.Add(TxPlcExpressionOperator.RightParenthesis);
        // Add the "*" operator.
        expressionBuilder.Add(TxPlcExpressionOperator.Multiply);
        // Add the multipler value.
        TxPlcValue v = new TxPlcValue();
        v.LRealValue = multiplier;
        expressionBuilder.Add(TxPlcSignalDataType.LReal, v);

        // Create the expression.
        return expressionBuilder.Expression;
    }

    /// <summary>
    /// Example 4 - Creates an expression demonstrating system function = RE(TRUE).
    /// </summary>
    /// <returns>The created expression.</returns>
    private static TxPlcExpression CreateExpression_SYSTEM_FUNCTION_RISING_EDGE()
    {
        TxPlcExpressionBuilder expressionBuilder = new TxPlcExpressionBuilder();

        // Create the function parameters - in this case a single value parameter.
        // Note that all parameters must be of TxPlcFunctionParameterData type.

        TxPlcFunctionParameterData functionParam = new TxPlcFunctionParameterData();
        TxPlcValue v = new TxPlcValue();
        v.BooleanValue = true;
        functionParam.SetAsValue(TxPlcSignalDataType.Bool, v);

        ArrayList functionParameters = new ArrayList() { functionParam };

        expressionBuilder.Add(TxPlcExpressionFunctionType.RE, functionParameters);

        // Create the expression.
        return expressionBuilder.Expression;
    }

    /// <summary>
    /// Example 5 - Creates an expression for user function, which parameter is an expression = ABS(COS(2.5)).
    /// </summary>
    /// <returns>The created expression.</returns>
    private static TxPlcExpression CreateExpression_USER_FUNCTION_WITH_EXPRESSION_PARAMETER()
    {
        TxPlcExpressionBuilder expressionBuilder = new TxPlcExpressionBuilder();

        // Create the function parameters - in this case a single expression parameter = COS(2.5).
        // Note that all parameters must be of TxPlcFunctionParameterData type.

        TxPlcFunctionParameterData functionParam = new TxPlcFunctionParameterData();
        // Use an expression as function parameter.
        TxPlcExpression expr = CreateExpression_COSINE_FUNCTION(2.5);
        functionParam.SetAsExpression(expr);

        ArrayList functionParameters = new ArrayList() { functionParam };

        string userFuncName = "ABS";
        expressionBuilder.Add(userFuncName, functionParameters);

        // Create the expression.
        return expressionBuilder.Expression;
    }

    /// <summary>
    /// Example 5.1 - Demonstrates creating an expression for a user function = COS(valueRad).
    /// </summary>
    /// <param name="valueRad">The value in radians.</param>
    /// <returns>The created expression.</returns>
    private static TxPlcExpression CreateExpression_COSINE_FUNCTION(double valueRad)
    {
        TxPlcExpressionBuilder expressionBuilder = new TxPlcExpressionBuilder();

        // Create the function parameters - in this case a single value parameter = valueRad.
        // Note that all parameters must be of TxPlcFunctionParameterData type.

        TxPlcFunctionParameterData functionParam = new TxPlcFunctionParameterData();
        TxPlcValue v = new TxPlcValue();
        v.LRealValue = valueRad;
        functionParam.SetAsValue(TxPlcSignalDataType.LReal, v);

        ArrayList functionParameters = new ArrayList() { functionParam };

        string userFuncName = "COS";
        expressionBuilder.Add(userFuncName, functionParameters);

        // Create the expression.
        return expressionBuilder.Expression;
    }

    /// <summary>
    /// Creates an LB entry with the given name & hardware type.
    /// </summary>
    /// <param name="lb">The Logic Behavior object.</param>
    /// <param name="name">The name of the entry.</param>
    /// <param name="hardwareType">The hardware type of the entry.</param>
    /// <returns>The created entry.</returns>
    /// <remarks>
    /// If already exists entry with this name - it is returned as is, diregarding the hardware type.
    /// </remarks>
    private static ITxPlcLogicBehaviorEntry CreateEntry(TxPlcLogicBehavior lb, string name, TxPlcHardwareType hardwareType)
    {
        // If already existing entry with this name - return it (diregarding the hardware type).
        foreach (ITxPlcLogicBehaviorEntry entry in lb.Entries)
        {
            if (entry.Name == name)
            {
                return entry;
            }
        }

        return lb.CreateEntry(new TxPlcLogicBehaviorEntryCreationData() { Name = name, HardwareType = hardwareType });
    }

    /// <summary>
    /// Creates an LB exit with the given name & hardware type.
    /// </summary>
    /// <param name="lb">The Logic Behavior object.</param>
    /// <param name="name">The name of the exit.</param>
    /// <param name="hardwareType">The hardware type of the exit.</param>
    /// <returns>The created exit.</returns>
    /// <remarks>
    /// If already exists exit with this name - it is returned as is, diregarding the hardware type.
    /// </remarks>
    private static ITxPlcLogicBehaviorExit CreateExit(TxPlcLogicBehavior lb, string name, TxPlcHardwareType hardwareType)
    {
        // If already existing exit with this name - return it (diregarding the hardware type).
        foreach (ITxPlcLogicBehaviorExit exit in lb.Exits)
        {
            if (exit.Name == name)
            {
                return exit;
            }
        }

        return lb.CreateExit(new TxPlcLogicBehaviorExitCreationData() { Name = name, HardwareType = hardwareType });
    }


    private static TxPlcLogicBehavior CreateLogicBehavior_OnResource(ITxComponent resource)
    {
        // Open the Resource for modeling (if not done already).
        if (!resource.IsOpenForModeling)
        {
            resource.SetModelingScope();
        }

        // Create the logic behavior on the resource.
        using (TxPlcLogicBehaviorCreationData lbCreationData = new TxPlcLogicBehaviorCreationData("LB"))
        {
            // Check if it's allowed to be created (validates whether a resource is passed + logic behavior is not yet created).
            if ((resource as ITxPlcLogicBehaviorCreation).CanCreateLogicBehavior(lbCreationData))
            {
                // Create the logic behavior associated with the resource.
                TxPlcLogicBehavior logicBehavior = (resource as ITxPlcLogicBehaviorCreation).CreateLogicBehavior(lbCreationData);
                return logicBehavior;
            }
        }

        return null;
    }

    //private static string ShowSelectionDialog(List<string> items)
    //{
    //    string selectedItem = null;

    //    using (Form form = new Form())
    //    {
    //        form.Text = "Select Logic Resource";
    //        form.Width = 400;
    //        form.Height = 200;

    //        ListBox listBox = new ListBox() { Dock = DockStyle.Fill };
    //        listBox.Items.AddRange(items.ToArray());

    //        Button okButton = new Button() { Text = "OK", Dock = DockStyle.Bottom };
    //        okButton.Click += (sender, e) =>
    //        {
    //            if (listBox.SelectedItem != null)
    //            {
    //                selectedItem = listBox.SelectedItem.ToString();
    //                form.DialogResult = DialogResult.OK;

    //                form.Close();
    //            }
    //            else
    //            {
    //                MessageBox.Show("Please select a resource.");
    //            }
    //        };

    //        form.Controls.Add(listBox);
    //        form.Controls.Add(okButton);

    //        form.ShowDialog();
    //    }

    //    return selectedItem;
    //}

    private static void RunTiaAndPlcSim()
    {
        try
        {
            // --- Load Siemens.Engineering.dll dynamically ---
            string engineeringDll = @"C:\Program Files\Siemens\Automation\Portal V17\PublicAPI\V17\Siemens.Engineering.dll";
            if (!File.Exists(engineeringDll))
            {
                //TxMessageBox.Show("TIA Engineering DLL not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Assembly tiaAssembly = Assembly.LoadFrom(engineeringDll);
            Type tiaType = tiaAssembly.GetType("Siemens.Engineering.TiaPortal");
            Type modeType = tiaAssembly.GetType("Siemens.Engineering.TiaPortalMode");
            object uiMode = Enum.Parse(modeType, "WithUserInterface");
            object tiaPortal = Activator.CreateInstance(tiaType, new object[] { uiMode });

            // Access Projects property
            PropertyInfo projectsProp = tiaType.GetProperty("Projects");
            object projects = projectsProp.GetValue(tiaPortal, null);
            Type projectsType = projects.GetType();

            // --- Open TIA project ---
            MethodInfo openMethod = projectsType.GetMethod("Open", new Type[] { typeof(FileInfo) });
            string projectPath = @"C:\Users\Public\Documents\VC Lite Tutorial\03_VC_Lite_Example_Automation_V17\03_VC_Lite_Example_Automation_V17.ap17";
            object project = openMethod.Invoke(projects, new object[] { new FileInfo(projectPath) });

            if (project == null)
            {
                //TxMessageBox.Show("Failed to open TIA project!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- Get Devices collection ---
            PropertyInfo devicesProp = project.GetType().GetProperty("Devices");
            object devices = devicesProp.GetValue(project, null);

            // --- Find CPU device item using recursive reflection ---
            object cpuDeviceItem = FindCpu(devices);
            if (cpuDeviceItem == null)
            {
                //TxMessageBox.Show("CPU not found in project!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //TxMessageBox.Show("? CPU device found successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // --- Get DownloadProvider via reflection ---
            MethodInfo getServiceMethod = cpuDeviceItem.GetType().GetMethod("GetService");
            Type downloadProviderType = tiaAssembly.GetType("Siemens.Engineering.Download.DownloadProvider");
            //object downloadProvider = getServiceMethod.Invoke(cpuDeviceItem, new object[] { downloadProviderType });
            // GetService<T> is generic, so make a closed version with the correct type argument
            MethodInfo genericGetService = getServiceMethod.MakeGenericMethod(downloadProviderType);

            // Now safely invoke it (no parameters)
            object downloadProvider = genericGetService.Invoke(cpuDeviceItem, null);
            // --- Load PLCSIM API dynamically ---
            string plcsimDll = @"C:\Program Files (x86)\Common Files\Siemens\PLCSIMADV\API\4.1\Siemens.Simatic.Simulation.Runtime.Api.x64.dll";
            if (!File.Exists(plcsimDll))
            {
                //TxMessageBox.Show("PLCSIM API DLL not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Assembly plcsimAssembly = Assembly.LoadFrom(plcsimDll);
            Type simManagerType = plcsimAssembly.GetType("Siemens.Simatic.Simulation.Runtime.SimulationRuntimeManager");
            Type cpuType = plcsimAssembly.GetType("Siemens.Simatic.Simulation.Runtime.ECPUType");
            Type instanceType = plcsimAssembly.GetType("Siemens.Simatic.Simulation.Runtime.IInstance");

            bool catche = false;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // --- Register PLCSIM instance ---
                    MethodInfo registerMethod = simManagerType.GetMethod("RegisterInstance", new Type[] { cpuType, typeof(string) });
                    object cpuEnum = Enum.Parse(cpuType, "CPU1500_Unspecified");
                    object plcInstance = registerMethod.Invoke(null, new object[] { cpuEnum, "PLC_1" });

                    // --- Power On ---
                    MethodInfo powerOnMethod = instanceType.GetMethod("PowerOn", new Type[] { typeof(uint) });
                    powerOnMethod.Invoke(plcInstance, new object[] { 60000u });
                    //TxMessageBox.Show("PLCSIM instance powered on successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    cheche = true;
                    MessageBox.Show("DLL Not Found", "Error");
                }
                if (catche == false)
                    break;
            }
            // --- Configure DownloadProvider and Download ---

            if (downloadProvider != null)
            {
                // Get Configuration property
                PropertyInfo configProp = downloadProvider.GetType().GetProperty("Configuration");
                object configuration = configProp.GetValue(downloadProvider, null);

                // Get Modes collection
                PropertyInfo modesProp = configuration.GetType().GetProperty("Modes");
                object modesObj = modesProp.GetValue(configuration, null);
                IEnumerable modes = modesObj as IEnumerable;

                if (modes == null)
                {
                    MessageBox.Show("No Modes found in configuration.", "Error");
                    return;
                }

                object configurationMode = null;

                // Find "PN/IE" mode
                foreach (var mode in modes)
                {
                    PropertyInfo nameProp = mode.GetType().GetProperty("Name");
                    string modeName = nameProp.GetValue(mode, null).ToString();
                    if (modeName == "PN/IE")
                    {
                        configurationMode = mode;
                        break;
                    }
                }

                if (configurationMode == null)
                {
                    MessageBox.Show("PN/IE mode not found.", "Error");
                    return;
                }

                // Find PC interface named "PLCSIM" with slot 1
                PropertyInfo pcInterfacesProp = configurationMode.GetType().GetProperty("PcInterfaces");
                object pcInterfacesObj = pcInterfacesProp.GetValue(configurationMode, null);
                IEnumerable pcInterfaces = pcInterfacesObj as IEnumerable;

                object pcInterface = null;

                foreach (var iface in pcInterfaces)
                {
                    PropertyInfo nameProp = iface.GetType().GetProperty("Name");
                    PropertyInfo numberProp = iface.GetType().GetProperty("Number");

                    string ifaceName = nameProp.GetValue(iface, null).ToString();
                    int ifaceNumber = (int)numberProp.GetValue(iface, null);

                    //Console.WriteLine($"Name: {ifaceName}, Slot: {ifaceNumber}");

                    if (ifaceName == "PLCSIM" && ifaceNumber == 1)
                    {
                        pcInterface = iface;
                    }
                }

                if (pcInterface == null)
                {
                    MessageBox.Show("PLCSIM PC interface not found.", "Error");
                    return;
                }

                // Get TargetInterfaces
                PropertyInfo targetIfacesProp = pcInterface.GetType().GetProperty("TargetInterfaces");
                object targetInterfacesObj = targetIfacesProp.GetValue(pcInterface, null);
                IList targetInterfaces = targetInterfacesObj as IList;
                object firstTarget = null;
                if (targetInterfacesObj != null)
                {
                    // Use IEnumerable to iterate
                    IEnumerable targetInterfacesEnum = targetInterfacesObj as IEnumerable;

                    if (targetInterfacesEnum != null)
                    {
                        // Get the first element

                        foreach (var t in targetInterfacesEnum)
                        {
                            firstTarget = t;
                            break; // only take the first
                        }

                        if (firstTarget != null)
                        {
                            PropertyInfo nameProp = firstTarget.GetType().GetProperty("Name");
                            string targetName = nameProp.GetValue(firstTarget, null).ToString();
                            // Console.WriteLine($"ConfigurationTargetInterface Name: {targetName}");
                        }
                        else
                        {
                            Console.WriteLine("TargetInterfaces is empty.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("TargetInterfaces does not implement IEnumerable.");
                    }
                }
                else
                {
                    Console.WriteLine("TargetInterfaces is null.");
                }


                //if (targetInterfaces == null || targetInterfaces.Count == 0)
                //{
                //    MessageBox.Show("TargetInterfaces is empty. Cannot proceed.", "Error");
                //    return;
                //}

                object targetConfiguration = firstTarget;

                // Optional: cast to ConfigurationTargetInterface if needed
                // object targetConfigurationIF = targetConfiguration;
                // Prepare DownloadOptions
                Type downloadOptionsType = downloadProvider.GetType().Assembly.GetType("Siemens.Engineering.Download.DownloadOptions");
                object loadOption = Enum.Parse(downloadOptionsType, "Hardware,Software");

                // Find Download method
                //MethodInfo downloadMethod = downloadProvider.GetType().GetMethod("Download");

                MethodInfo[] methods = downloadProvider.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                MethodInfo downloadMethod = null;

                foreach (var m in methods)
                {
                    if (m.Name == "Download")
                    {
                        ParameterInfo[] parameters = m.GetParameters();
                        // Choose the overload with at least 1 parameter (targetConfiguration)
                        if (parameters.Length >= 1)
                        {
                            downloadMethod = m;
                            break; // stop at the first suitable method
                        }
                    }
                }

                if (downloadMethod != null)
                {
                    Type downloadDelegateType = downloadProvider.GetType().Assembly.GetType("Siemens.Engineering.Download.DownloadConfigurationDelegate");

                    Type downloadConfigType = downloadProvider.GetType().Assembly.GetType("Siemens.Engineering.Download.DownloadConfiguration");

                    Type currentType = typeof(ExampleFindOrCreateLogicResource);

                    MethodInfo preMethod = currentType.GetMethod("PreConfigureDownload", BindingFlags.NonPublic | BindingFlags.Static);
                    MethodInfo postMethod = currentType.GetMethod("PostConfigureDownload", BindingFlags.NonPublic | BindingFlags.Static);

                    if (preMethod == null || postMethod == null)
                    {
                        Console.WriteLine("? Pre/Post methods not found. Check method names and access modifiers.");
                    }

                    // 3?? Create delegate instances dynamically
                    object preDownloadDelegate = Delegate.CreateDelegate(downloadDelegateType, preMethod);
                    object postDownloadDelegate = Delegate.CreateDelegate(downloadDelegateType, postMethod);

                    object result = downloadMethod.Invoke(downloadProvider, new object[] { targetConfiguration, preDownloadDelegate, postDownloadDelegate, loadOption });
                    //object preDelegate = null;
                    //object postDelegate = null;

                    //if (preMethod != null && downloadConfigDelegateType != null)
                    //{
                    //    preDelegate = Delegate.CreateDelegate(downloadConfigDelegateType, preMethod);
                    //}

                    //if (postMethod != null && downloadConfigDelegateType != null)
                    //{
                    //    postDelegate = Delegate.CreateDelegate(downloadConfigDelegateType, postMethod);
                    //}


                    //object preDelegate = Delegate.CreateDelegate(downloadConfigDelegateType, preMethod);
                    //object postDelegate = Delegate.CreateDelegate(downloadConfigDelegateType, postMethod);


                    //object result = downloadMethod.Invoke(downloadProvider,new object[] { targetConfiguration, preDelegate, postDelegate, loadOption });
                    // Invoke download with null delegates (you can replace with reflection-based delegates if needed)
                    //object result = downloadMethod.Invoke(downloadProvider, new object[] { targetConfiguration, null, null, loadOption });
                    MessageBox.Show("Download executed successfully!", "Info");
                }
                else
                {
                    MessageBox.Show("Download method not found.", "Error");
                }
            }

            //if (downloadProvider != null)
            //{
            //    PropertyInfo configProp = downloadProvider.GetType().GetProperty("Configuration");
            //    object configuration = configProp.GetValue(downloadProvider, null);

            //    PropertyInfo legacyProp = configuration.GetType().GetProperty("EnableLegacyCommunication");
            //    legacyProp?.SetValue(configuration, true);

            //    PropertyInfo modesProp = configuration.GetType().GetProperty("Modes");
            //    IEnumerable modes = modesProp.GetValue(configuration) as IEnumerable;

            //    foreach (var mode in modes)
            //    {
            //        PropertyInfo pcInterfacesProp = mode.GetType().GetProperty("PcInterfaces");
            //        IEnumerable pcInterfaces = pcInterfacesProp.GetValue(mode) as IEnumerable;

            //        foreach (var pcInterface in pcInterfaces)
            //        {
            //            PropertyInfo nameProp = pcInterface.GetType().GetProperty("Name");
            //            string name = nameProp.GetValue(pcInterface)?.ToString();
            //            if (name == "PLCSIM")
            //            {
            //                PropertyInfo targetIfacesProp = pcInterface.GetType().GetProperty("TargetInterfaces");
            //                IList targetInterfaces = targetIfacesProp.GetValue(pcInterface, null) as IList;
            //                object targetConfigurationIF = targetInterfaces[0];

            //                PropertyInfo addressesProp = targetConfigurationIF.GetType().GetProperty("Addresses");
            //                object addresses = addressesProp.GetValue(targetConfigurationIF, null);
            //                MethodInfo createMethod = addresses.GetType().GetMethod("Create");
            //                createMethod.Invoke(addresses, new object[] { "192.168.0.1" });

            //                // Setup delegates and download
            //                Type downloadOptionsType = plcsimAssembly.GetType("Siemens.Simatic.Simulation.Runtime.DownloadOptions");
            //                object loadOption = Enum.Parse(downloadOptionsType, "Hardware,Software");

            //                MethodInfo downloadMethod = downloadProvider.GetType().GetMethod("Download");
            //                downloadMethod.Invoke(downloadProvider, new object[] { targetConfigurationIF, null, null, loadOption });

            //                //TxMessageBox.Show("Download executed successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //            }
            //        }
            //    }
            //}

            //TxMessageBox.Show("? TIA Project and PLCSIM setup completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            TxMessageBox.Show("Error: " + ex.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void PreConfigureDownload(object downloadConfiguration)
    {
        if (downloadConfiguration == null)
            return;

        Type configType = downloadConfiguration.GetType();
        string typeName = configType.FullName;

        try
        {
            // 1. StopModules case
            if (typeName.Contains("StopModules"))
            {
                PropertyInfo selectionProp = configType.GetProperty("CurrentSelection");
                if (selectionProp != null)
                {
                    Type enumType = selectionProp.PropertyType;
                    object stopAllEnum = Enum.Parse(enumType, "StopAll");
                    selectionProp.SetValue(downloadConfiguration, stopAllEnum, null);
                    Console.WriteLine("[PreConfigure] PLC set to STOP mode.");
                }
                return;
            }

            // 2. AlarmTextLibrariesDownload case
            if (typeName.Contains("AlarmTextLibrariesDownload"))
            {
                PropertyInfo selectionProp = configType.GetProperty("CurrentSelection");
                if (selectionProp != null)
                {
                    Type enumType = selectionProp.PropertyType;
                    object consistentDownload = Enum.Parse(enumType, "ConsistentDownload");
                    selectionProp.SetValue(downloadConfiguration, consistentDownload, null);
                    Console.WriteLine("[PreConfigure] Alarm text libraries set to ConsistentDownload.");
                }
                return;
            }

            // 3. BlockBindingPassword case
            if (typeName.Contains("BlockBindingPassword"))
            {
                MethodInfo setPwd = configType.GetMethod("SetPassword");
                if (setPwd != null)
                {
                    var securePwd = CreateSecureString("1234"); // Example password
                    setPwd.Invoke(downloadConfiguration, new object[] { securePwd });
                    Console.WriteLine("[PreConfigure] Block binding password applied.");
                }
                return;
            }

            // 4. CheckBeforeDownload case
            if (typeName.Contains("CheckBeforeDownload"))
            {
                PropertyInfo checkedProp = configType.GetProperty("Checked");
                if (checkedProp != null)
                {
                    checkedProp.SetValue(downloadConfiguration, true, null);
                    Console.WriteLine("[PreConfigure] CheckBeforeDownload enabled.");
                }
                return;
            }

            // 5. ConsistentBlocksDownload case
            if (typeName.Contains("ConsistentBlocksDownload"))
            {
                PropertyInfo selectionProp = configType.GetProperty("CurrentSelection");
                if (selectionProp != null)
                {
                    Type enumType = selectionProp.PropertyType;
                    object consistentDownload = Enum.Parse(enumType, "ConsistentDownload");
                    selectionProp.SetValue(downloadConfiguration, consistentDownload, null);
                    Console.WriteLine("[PreConfigure] Consistent block download enabled.");
                }
                return;
            }

            // 6. ModuleWriteAccessPassword case
            if (typeName.Contains("ModuleWriteAccessPassword"))
            {
                MethodInfo setPwd = configType.GetMethod("SetPassword");
                if (setPwd != null)
                {
                    var securePwd = CreateSecureString("4321"); // Example password
                    setPwd.Invoke(downloadConfiguration, new object[] { securePwd });
                    Console.WriteLine("[PreConfigure] Module write access password applied.");
                }
                return;
            }

            throw new NotSupportedException("Unsupported configuration type : " + typeName);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[PreConfigure] Exception: " + ex.Message);
            throw;
        }
    }

    private static void PostConfigureDownload(object downloadConfiguration)
    {
        if (downloadConfiguration == null)
            return;

        Type configType = downloadConfiguration.GetType();
        string typeName = configType.FullName;

        try
        {
            // StartModules case
            if (typeName.Contains("StartModules"))
            {
                PropertyInfo selectionProp = configType.GetProperty("CurrentSelection");
                if (selectionProp != null)
                {
                    Type enumType = selectionProp.PropertyType;
                    object startModule = Enum.Parse(enumType, "StartModule");
                    selectionProp.SetValue(downloadConfiguration, startModule, null);
                    Console.WriteLine("[PostConfigure] PLC set to RUN mode.");
                }
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[PostConfigure] Exception: " + ex.Message);
            throw;
        }
    }

    // Helper: create SecureString dynamically
    private static System.Security.SecureString CreateSecureString(string plainText)
    {
        var secure = new System.Security.SecureString();
        foreach (char c in plainText)
            secure.AppendChar(c);
        secure.MakeReadOnly();
        return secure;
    }


    private static object FindCpu(object devices)
    {
        IEnumerable deviceEnumerable = devices as IEnumerable;
        if (deviceEnumerable != null)
        {
            foreach (var device in deviceEnumerable)
            {
                // Get DeviceItems property
                PropertyInfo deviceItemsProp = device.GetType().GetProperty("DeviceItems");
                object deviceItems = deviceItemsProp.GetValue(device);

                // Search recursively for CPU
                object cpu = SearchDeviceItems(deviceItems);
                if (cpu != null)
                    return cpu;
            }
        }
        return null;
    }

    private static object SearchDeviceItems(object deviceItems)
    {
        IEnumerable itemsEnumerable = deviceItems as IEnumerable;
        if (itemsEnumerable != null)
        {
            foreach (var item in itemsEnumerable)
            {
                PropertyInfo classificationProp = item.GetType().GetProperty("Classification");
                string classification = classificationProp.GetValue(item).ToString();

                if (!string.IsNullOrEmpty(classification) && classification.Contains("CPU"))
                {
                    return item; // ? Found CPU
                }

                PropertyInfo subItemsProp = item.GetType().GetProperty("DeviceItems");
                object subItems = subItemsProp.GetValue(item);
                object found = SearchDeviceItems(subItems);
                if (found != null)
                    return found;
            }
        }
        return null;
    }
    private static Assembly AssemblyResolver(object sender, ResolveEventArgs args)
    {
        string[] folders = new string[]
        {
                @"C:\Program Files\Siemens\Automation\Portal V17\PublicAPI\V17",
                @"C:\Program Files\Siemens\Automation\Portal V17\Bin\PublicAPI",
                @"C:\Program Files (x86)\Common Files\Siemens\PLCSIMADV\API\4.1"
        };

        string requestedName = new AssemblyName(args.Name).Name + ".dll";
        foreach (var folder in folders)
        {
            string path = Path.Combine(folder, requestedName);
            if (File.Exists(path))
            {
                return Assembly.LoadFrom(path);
            }
        }
        return null;
    }

    #endregion Private Methods
}


