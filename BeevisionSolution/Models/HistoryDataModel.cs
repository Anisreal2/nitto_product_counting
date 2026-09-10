using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    public class HistoryDataModel
    {
        public string LogTime { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public static HistoryDataModel FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            HistoryDataModel data = new HistoryDataModel();
            data.LogTime = values[0];
            data.Title = values[1];
            data.Code = values[2];
            data.Type = values[3];
            data.Description = values[4];
            return data;
        }

        public string GetRawData()
        {
            return string.Format(@"{0},{1},{2},{3}", Title, Code, Type, Description);
        }
        public HistoryDataModel() { }
        public HistoryDataModel(string code)
        {
            HistoryDataModel d = ErrorDefine.GetDataByCode(code);
            if (d != null)
            {
                Title = d.Title;
                Code = code;
                Type = d.Type;
                Description = d.Description;
            }
        }
        public HistoryDataModel(string code, string title, string type, string description)
        {
            Title = title;
            Code = code;
            Type = type;
            Description = description;
        }
    }

    class ErrorDefine
    {
        public static List<HistoryDataModel> LstErrorDefine = new List<HistoryDataModel> { };
        public static void Init()
        {
            LstErrorDefine.Add(new HistoryDataModel("0", "Sample Title", "ErrorStop", "Error Define Sample."));
            LstErrorDefine.Add(new HistoryDataModel("1", "Module Permission NG", "ErrorStop", "Module is not available. (Need to check dongle key)."));
            LstErrorDefine.Add(new HistoryDataModel("2", "Module Validation NG", "ErrorStop", "The setup of the Module is incomplete."));
            LstErrorDefine.Add(new HistoryDataModel("3", "Running Connector Null", "ErrorStop", "The Connector name to run is invalid."));
            LstErrorDefine.Add(new HistoryDataModel("4", "Attached Connection Is Empty", "Warning", "The Connector does not have a Connection to execute."));
            LstErrorDefine.Add(new HistoryDataModel("5", "Exception Module", "ErrorStop", "An exception occurred while executing Module."));
            LstErrorDefine.Add(new HistoryDataModel("6", "Information is insufficient", "ErrorStop", "Module does not have enough information to run."));
            LstErrorDefine.Add(new HistoryDataModel("100", "Error code not defined", "ErrorStop", "Error Code is not defined."));
            LstErrorDefine.Add(new HistoryDataModel("201", "Grab_Fail", "ErrorStop", "Camera Grab failed."));
            LstErrorDefine.Add(new HistoryDataModel("202", "Camera is Null", "ErrorStop", "Camera is not set."));
            LstErrorDefine.Add(new HistoryDataModel("203", "Display is Null", "ErrorStop", "Display is not set."));
            LstErrorDefine.Add(new HistoryDataModel("204", "OutputImage is Null", "ErrorStop", "OutputImage is null."));
            LstErrorDefine.Add(new HistoryDataModel("205", "OutputImage1 is Null", "ErrorStop", "OutputImage1 is null."));
            LstErrorDefine.Add(new HistoryDataModel("206", "OuputImages are Null", "ErrorStop", "OutputImages are all null."));
            LstErrorDefine.Add(new HistoryDataModel("2101", "Invalid Property Settings", "ErrorStop", "Property setting error."));
            LstErrorDefine.Add(new HistoryDataModel("2201", "Invalid Property Settings", "ErrorStop", "Property setting error."));
            LstErrorDefine.Add(new HistoryDataModel("2202", "Invalid Chess Size Settings", "ErrorStop", "Chess Size setting error."));
            LstErrorDefine.Add(new HistoryDataModel("2203", "Invalid Chess Count Settings", "ErrorStop", "Chess Count is not correct. Set the JIG horizontally."));
            LstErrorDefine.Add(new HistoryDataModel("10001", "Invalid input command value", "ErrorStop", "The command value entered in the previous module is invalid."));
            LstErrorDefine.Add(new HistoryDataModel("10002", "Invalid AlignTable Settings", "ErrorStop", "The communication table settings are incorrect."));
            LstErrorDefine.Add(new HistoryDataModel("10003", "Invalid Comunicator Settings", "ErrorStop", "Communication module settings are incorrect."));
            LstErrorDefine.Add(new HistoryDataModel("10004", "Failed PLC Area Update", "ErrorStop", "Communication PLC area update failed."));
            LstErrorDefine.Add(new HistoryDataModel("10005", "Failed Vision Area Update", "ErrorStop", "Communication Vision area update failed."));
            LstErrorDefine.Add(new HistoryDataModel("10006", "Failed PLC Area Write", "ErrorStop", "Communication PLC area write failed."));
            LstErrorDefine.Add(new HistoryDataModel("10007", "Failed Vision Area Write", "ErrorStop", "Communication Vision area write failed."));
            LstErrorDefine.Add(new HistoryDataModel("10008", "Failed Vision Bit On Write", "ErrorStop", "Communication Vision Bit On write failed."));
            LstErrorDefine.Add(new HistoryDataModel("10009", "Failed Vision Bit Off Write", "ErrorStop", "Communication Vision Bit Off write failed."));
            LstErrorDefine.Add(new HistoryDataModel("10010", "PLC Bit On Time Out", "ErrorStop", "PLC Bit On waiting timed out."));
            LstErrorDefine.Add(new HistoryDataModel("10011", "PLC Bit Off Time Out", "ErrorStop", "PLC Bit Off waiting timed out."));
            LstErrorDefine.Add(new HistoryDataModel("10012", "value type differs", "ErrorStop", "The type of the input value and the set value are different."));
            LstErrorDefine.Add(new HistoryDataModel("10013", "IO Is Null", "ErrorStop", "IO is not set. (Need to check IO Table)"));
            LstErrorDefine.Add(new HistoryDataModel("10601", "Command Connector Is Null", "ErrorStop", "No Connector was found to run."));
            LstErrorDefine.Add(new HistoryDataModel("10611", "Mark ROI NG", "Warning", "Mark Find Fail. (Mark ROI)"));
            LstErrorDefine.Add(new HistoryDataModel("10612", "Mark NG", "Warning", "Mark Find Fail. (Mark NG)"));
            LstErrorDefine.Add(new HistoryDataModel("10613", "L Check NG", "Warning", "Mark Find Fail. (L-Check)"));
            LstErrorDefine.Add(new HistoryDataModel("10614", "No Mark", "Warning", "Mark Find Fail. (No Mark)"));
            LstErrorDefine.Add(new HistoryDataModel("10615", "Vision Limit", "ErrorStop", "Vision Align Limit allowance exceeded."));
            LstErrorDefine.Add(new HistoryDataModel("10701", "Aqcuisition tool is not attached", "ErrorStop", "Acquisiiton Tool is not connected."));
            LstErrorDefine.Add(new HistoryDataModel("10702", "Operator Call(Manual Mark)", "OpCall", "Manual mark location teaching is requested to the operator."));
            LstErrorDefine.Add(new HistoryDataModel("10801", "Failed to retrieve location value.", "ErrorStop", "Failed to read the location value. (value type covert fail or key not include)"));
            LstErrorDefine.Add(new HistoryDataModel("10802", "Failed to position set Values.", "ErrorStop", "Target position setting failed. (Target Position IO setting failed)"));
            LstErrorDefine.Add(new HistoryDataModel("11301", "Aqcuisition tool is not attached", "ErrorStop", "Acquisiiton Tool is not connected."));
            LstErrorDefine.Add(new HistoryDataModel("11302", "Operator Call(Manual Mark)", "OpCall", "Manual mark location teaching is requested to the operator."));
            LstErrorDefine.Add(new HistoryDataModel("15001", "CalData is Null", "ErrorStop", "Calibration Data is not set."));
            LstErrorDefine.Add(new HistoryDataModel("15002", "Tolerance Exceeded", "Warning", "The correction value is out of tolerance."));
            LstErrorDefine.Add(new HistoryDataModel("15003", "Mark Info Missing", "Warning", "Insufficient Position information to calculate."));
            LstErrorDefine.Add(new HistoryDataModel("15004", "Length Check", "Warning", "Length Check Fail. It is necessary to check the location of the material size or mark."));
            LstErrorDefine.Add(new HistoryDataModel("15005", "InputData Info Missing", "Warning", "There is not enough InputData information to operate."));
            LstErrorDefine.Add(new HistoryDataModel("15010", "Failed to obtain Inspection InputData Value Value.", "ErrorStop", "Failed to read Input Data to Inspection."));
            LstErrorDefine.Add(new HistoryDataModel("15011", "Failed to Calculation Distance.", "ErrorStop", "Failed to calculate distance for inspection."));
            LstErrorDefine.Add(new HistoryDataModel("15012", "Angle Tolerance Exceeded", "ErrorStop", "The Angle Tool value was exceeded."));
            LstErrorDefine.Add(new HistoryDataModel("15101", "PMAling Mark1 NG", "Warning", "PMAlign No. 1 Mark could not be found."));
            LstErrorDefine.Add(new HistoryDataModel("15102", "PMAling Mark2 NG", "Warning", "PMAlign No. 2 Mark could not be found."));
            LstErrorDefine.Add(new HistoryDataModel("15103", "PMAling Mark1,2 NG", "ErrorStop", "PMAlign could not find any Marks."));
            LstErrorDefine.Add(new HistoryDataModel("15104", "PMAling the Whole Mark NG", "ErrorStop", "PMAlign Poperty setting is incorrect. (Skip is not used in Single Flow)"));
            LstErrorDefine.Add(new HistoryDataModel("15401", "Failed to obtain Coordinate value.", "ErrorStop", "Failed to read the value of each Coordinate coordinate."));
            LstErrorDefine.Add(new HistoryDataModel("15402", "Failed to upload FDC VisionPos_Value.", "ErrorStop", "Failed to upload each FDC VisionPos."));
            LstErrorDefine.Add(new HistoryDataModel("15403", "Failed to upload FDC Data value(position).", "ErrorStop", "Failed to upload FDC data for each position."));
            LstErrorDefine.Add(new HistoryDataModel("15404", "Failed to upload L_Check Data value.", "ErrorStop", "Failed to upload FDC data for each L_Check."));
            LstErrorDefine.Add(new HistoryDataModel("15405", "Failed to upload L_Check Data value.", "ErrorStop", "Failed to upload FDC data for each TarRevPos."));
            LstErrorDefine.Add(new HistoryDataModel("15406", "Failed to upload L_Check Data value.", "ErrorStop", "Failed to upload FDC data for each RevPos."));
            LstErrorDefine.Add(new HistoryDataModel("15407", "Failed to obtain VisionPos value.", "ErrorStop", "Failed to read each Camera Vision coordinate value."));
            LstErrorDefine.Add(new HistoryDataModel("15408", "Failed to obtain CalData value.", "ErrorStop", "Failed to read the Cal data value of each Camera."));
            LstErrorDefine.Add(new HistoryDataModel("15409", "Failed to obtain CalData value.", "ErrorStop", "Failed to read 'Calibration' Position value of each Camera."));
            LstErrorDefine.Add(new HistoryDataModel("15410", "Failed to obtain CurData value.", "ErrorStop", "Failed to read the 'Current' Position value of each Camera."));
            LstErrorDefine.Add(new HistoryDataModel("15411", "Failed to obtain CurData value.", "ErrorStop", "Failed to read offset value."));
            LstErrorDefine.Add(new HistoryDataModel("15412", "Failed to obtain LCheck Spec In value.", "ErrorStop", "L-Check Spec Out."));
            LstErrorDefine.Add(new HistoryDataModel("15413", "Failed to obtain Camera CalPos Value.", "ErrorStop", "Failed to read Cam CalPos Value from Calibraion."));
            LstErrorDefine.Add(new HistoryDataModel("16101", "InputData Name Missing(BlobResults)", "Warning", "BlobResult not found in InputData."));
            LstErrorDefine.Add(new HistoryDataModel("16103", "Blob Count NG", "Warning", "The number of blobs or holes is out of specification."));
            LstErrorDefine.Add(new HistoryDataModel("16104", "Blob Area NG", "Warning", "The size of the entire area of the blob or hole is out of specification."));
            LstErrorDefine.Add(new HistoryDataModel("16105", "Blob Count & Blob Area NG", "Warning", "The number of blobs or holes and the total area size are below specification."));
            LstErrorDefine.Add(new HistoryDataModel("16201", "PMAling Mark1 NG", "Warning", "JIG PMAlign No. 1 Mark could not be found."));
            LstErrorDefine.Add(new HistoryDataModel("16202", "PMAling Mark2 NG", "Warning", "JIG PMAlign No. 2 Mark could not be found."));
            LstErrorDefine.Add(new HistoryDataModel("16203", "Reference Point NG", "ErrorStop", "Reference value is not set."));
            LstErrorDefine.Add(new HistoryDataModel("16204", "Cal Data NG", "ErrorStop", "Cal Data is not set."));
            LstErrorDefine.Add(new HistoryDataModel("16205", "Unit Number NG", "ErrorStop", "Unit number value is out of range. (range : 1~4)"));
            LstErrorDefine.Add(new HistoryDataModel("16206", "PMAling the Whole Mark NG", "ErrorStop", "PMAlign Poperty setting is incorrect. (PM Align Skip is not used when JIG Skip mode is OFF)"));
            LstErrorDefine.Add(new HistoryDataModel("19001", "Image is Null", "ErrorStop", "Input Image is null."));
            LstErrorDefine.Add(new HistoryDataModel("19002", "Mark_Not_Find", "ErrorStop", "Mark not found."));
            LstErrorDefine.Add(new HistoryDataModel("19003", "Display is Null", "ErrorStop", "Display is not set."));
            LstErrorDefine.Add(new HistoryDataModel("19004", "Operator Abort(Manual Mark Cancle)", "ErrorStop", "The operator canceled manual mark position teaching."));
            LstErrorDefine.Add(new HistoryDataModel("19006", "L_Check_NG", "OpCall", "Manual mark location teaching is requested to the operator."));
            LstErrorDefine.Add(new HistoryDataModel("19010", "[Cancle]Processing_Stop", "ErrorStop", "Process terminated during manual mark operation."));
            LstErrorDefine.Add(new HistoryDataModel("19011", "[Cancle]Unknown", "ErrorStop", "The operator canceled the manual mark operation because the cause was unknown."));
            LstErrorDefine.Add(new HistoryDataModel("19012", "[Cancle]Damage", "ErrorStop", "The operator canceled the manual mark operation because the mark was damaged."));
            LstErrorDefine.Add(new HistoryDataModel("19013", "[Cancle]Contamination", "ErrorStop", "The operator canceled the manual mark operation because the mark was contaminated."));
            LstErrorDefine.Add(new HistoryDataModel("19014", "[Cancle]Out_of_ROI", "ErrorStop", "The operator canceled the manual mark operation because the mark was out of the ROI."));
            LstErrorDefine.Add(new HistoryDataModel("19015", "[Cancle]No_Mark", "ErrorStop", "The operator canceled the manual mark operation because there was no mark."));
            LstErrorDefine.Add(new HistoryDataModel("19016", "[Cancle]Low_Recognition_Rate", "ErrorStop", "The mark recognition rate was low, so the operator canceled the manual mark operation."));
            LstErrorDefine.Add(new HistoryDataModel("19017", "[Cancle]Shake", "ErrorStop", "The operator canceled the manual mark operation because the mark was shaking."));
            LstErrorDefine.Add(new HistoryDataModel("19018", "[Cancle]Focus_Blur", "ErrorStop", "The operator canceled the manual mark operation because the focus of the mark was blurred."));
            LstErrorDefine.Add(new HistoryDataModel("19019", "[Cancle]Abnormality_of_shape_Mark", "ErrorStop", "The operator canceled the manual mark operation because the shape of the mark was different."));
            LstErrorDefine.Add(new HistoryDataModel("20001", "No Terminal", "ErrorStop", "No Output Terminal required for Tool Running."));
            LstErrorDefine.Add(new HistoryDataModel("20002", "No Terminal", "ErrorStop", "Tool execution result is Accept, but Terminal Value is Null."));
            LstErrorDefine.Add(new HistoryDataModel("20003", "Terminal Type Different", "ErrorStop", "Tool execution result is Accept, but Terminal Value Type is different."));
            LstErrorDefine.Add(new HistoryDataModel("20004", "LineFind Failed", "ErrorStop", "As a result of tool execution, finding the intersection of lines has failed."));
            LstErrorDefine.Add(new HistoryDataModel("20501", "Mark is not trained", "ErrorStop", "There is an unregistered Mark in the Tool List."));
            LstErrorDefine.Add(new HistoryDataModel("20601", "A Line Not Found", "ErrorStop", "Line could not be found."));
            LstErrorDefine.Add(new HistoryDataModel("20602", "B Line Not Found", "ErrorStop", "Line could not be found."));
            LstErrorDefine.Add(new HistoryDataModel("20603", "C Line Not Found", "ErrorStop", "Line could not be found."));
            LstErrorDefine.Add(new HistoryDataModel("20604", "D Line Not Found", "ErrorStop", "Line could not be found."));
            LstErrorDefine.Add(new HistoryDataModel("20605", "Mark Not Found", "ErrorStop", "Mark could not be found."));
            LstErrorDefine.Add(new HistoryDataModel("20606", "AB Cross Line Angle NG", "ErrorStop", "The angle of the intersection of A and B lines exceeds the allowable value."));
            LstErrorDefine.Add(new HistoryDataModel("20607", "BC Cross Line Angle NG", "ErrorStop", "The angle of the intersection of B and C lines exceeds the allowable value."));
            LstErrorDefine.Add(new HistoryDataModel("20608", "CD Cross Line Angle NG", "ErrorStop", "The angle of the intersection of C and D lines exceeds the allowable value."));
            LstErrorDefine.Add(new HistoryDataModel("20609", "DA Cross Line Angle NG", "ErrorStop", "The angle of the intersection of D and A lines exceeds the allowable value."));

        }

        public static HistoryDataModel GetDataByCode(string code)
        {
            if (LstErrorDefine.Count == 0)
            {
                ErrorDefine.Init();
            }
            return LstErrorDefine.FirstOrDefault(p => p.Code == code);
        }

    }
}
