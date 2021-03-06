﻿using NetCore2Blockly.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCore2Blockly.JavascriptGeneration
{
    /// <summary>
    /// generates items to toolbox
    /// </summary>
    public class BlocklyToolBoxFunctionDefinitionsGenerator
    {
        /// <summary>
        /// Generates the blockly tool box function definitions.
        /// </summary>
        /// <param name="actionList">The action list.</param>
        /// <param name="key">The action list.</param>
        /// <returns></returns>
        public string GenerateBlocklyToolBoxFunctionDefinitions(List<ActionInfo> actionList,string key)
        {
            var dictOrder = new Dictionary<string,int>()
            {
                {"get",1 },
                {"post" ,2},
                {"put" ,3},
                {"delete",4 },
                {"others",5 }

            };
            actionList.Sort((a, b) => {
                var res = a.ControllerName.CompareTo(b.ControllerName);
                if (res != 0)
                    return res;

                var v1 = dictOrder.ContainsKey(a.Verb.ToLower()) ? dictOrder[a.Verb.ToLower()] : dictOrder["others"];
                var v2 = dictOrder.ContainsKey(b.Verb.ToLower()) ? dictOrder[b.Verb.ToLower()] : dictOrder["others"];

                
                res = v1.CompareTo(v2);
                if (res != 0)
                    return res;

                return a.ActionName.CompareTo(b.ActionName);

                });
            string blockText = $"var blockTextLocalSiteFunctions{key}='';";
            foreach (var actionsGroupedByController in actionList.GroupBy(it => it.ControllerName))
            {
                var controllerName = actionsGroupedByController.Key;
                var actionHash = actionList.First(it=>it.ControllerName==controllerName).CustomGetHashCode();

                var blockColor = BlocklyStringToColor.ConvertToHue(actionHash);

                blockText += $"blockTextLocalSiteFunctions{key} += '<category name=\"{controllerName}\" colour=\"{blockColor}\">' ; ";
                foreach (var action in actionsGroupedByController)
                {
                    blockText += $@"{Environment.NewLine}
                        blockTextLocalSiteFunctions{key} += '<block type=""text_print""> <value name=""TEXT""><block type=""{action.GenerateCommandName()}"">';";
                    if (action.HasParams)
                    {
                        foreach (var param in action.Params)
                        {
                            var type = param.Value.type;
                            var blocklyParameterName = param.Key;
                            if (type.ConvertibleToBlocklyType())
                            {


                                var blockShadowType = type.TranslateToBlocklyBlocksType();
                                blockText += $@"{Environment.NewLine}
                                                 blockTextLocalSiteFunctions{key} += '<value name=""val_{blocklyParameterName}""><shadow type=""{blockShadowType}"">{GenerateBlockShadowField(blockShadowType)}</shadow></value>';  
                                              ";


                            }
                            else
                            {
                                var typeWithoutBlocklyType = actionList.GetAllTypesWithNullBlocklyType().FirstOrDefault(x => x == type);
                                if (typeWithoutBlocklyType != null)
                                {
                                    var blockShadowType = type.TranslateToNewTypeName();
                                    blockText += $@"{Environment.NewLine}
                                                    blockTextLocalSiteFunctions{key} += '<value name=""val_{blocklyParameterName}""><shadow type=""{blockShadowType}""></shadow></value>';                                                    
                                                   ";


                                }

                            }
                        }
                    }

                    blockText += $"blockTextLocalSiteFunctions{key} += '</block></block>';";
                }
                blockText += $"blockTextLocalSiteFunctions{key}+='</category>';";

            }

            return blockText;
        }

        /// <summary>
        /// Generates the block shadow field.
        /// </summary>
        /// <param name="blockShadowType">Type of the block shadow.</param>
        /// <returns></returns>
        public string GenerateBlockShadowField(string blockShadowType)
        {
            switch (blockShadowType)
            {
                case "math_number":
                    
                    return $@"<field name=""NUM"">0</field>";

                case "text":

                    return $@"<field name=""TEXT"">This is a text</field>";

                default:
                    return "";
            }
        }
    }
}
