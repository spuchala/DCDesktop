using edu.stanford.nlp.process;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.trees;
using edu.stanford.nlp.parser.lexparser;
using java.util;
using System.Collections.Generic;
using java.io;
using System.Web;
using DayCareWebAPI.Models;
using DayCareWebAPI.Repository;
using System;

namespace DayCareWebAPI.Services
{
    public class AssistantService
    {
        public Tree VPTree { get; set; }
        public string possessive { get; set; }

        public string Process(Assistant input)
        {
            //insert instant log message if its not a command
            var _repo = new DayCareRepository();
            var _serv = new DayCareService();
            if (!input.IsCommand)
            {
                var brandnewReport = true;
                var message = new Message()
                {
                    KidId = input.KidId,
                    Type = input.Predicate,
                    Value = input.Message,
                    Time = !string.IsNullOrEmpty(input.Time) ? input.Time : DateTime.Now.ToString("hh:mm tt")
                };
                _repo.InsertInstantLogMessage(message, input.KidId, null);
                //add message to report
                var log = _serv.GetKidLogOnADay(input.KidId, input.DayCareId, DateTime.Today.ToString("yyyy-MM-dd"), false);
                if (log == null)
                {
                    log = new KidLog() { KidId = input.KidId };
                    InitializeKidLog(log);
                    brandnewReport = true;
                }
                else
                    brandnewReport = false;
                ProcessHelper(log, input, brandnewReport);
                _serv.InsertKidLog(log);
            }
            else
            {
                ProcessCommand(input, _serv);
            }
            return string.Empty;
        }

        private string ProcessCommand(Assistant input, DayCareService _serv)
        {
            switch (input.Predicate.ToLower())
            {
                case Constants.Delete:
                    return _serv.RemoveKid(input.KidId, input.DayCareId, "Removing from speech assistant");
                case Constants.Email:
                    break;
                case Constants.Edit:
                    input.DestUrl = Constants.EditKidUrlForMobileApp + input.KidId + "/" + input.DayCareId + "/" + string.Empty;
                    break;
                case Constants.Update:
                    input.DestUrl = Constants.EditKidUrlForMobileApp + input.KidId + "/" + input.DayCareId + "/" + string.Empty;
                    break;
                case Constants.Insert:
                    break;
                case Constants.Add:
                    break;
                default:
                    break;
            }
            return string.Empty;
        }

        private void InitializeKidLog(KidLog log)
        {
            log.Foods = new List<Food>();
            log.Pottys = new List<Potty>();
            log.Naps = new List<Nap>();
            log.Activities = new List<Activity>();
            log.ProblemsConcerns = log.SuppliesNeeded = log.Comments = string.Empty;
            log.Foods.Add(new Food() { WhatKidAte = string.Empty, WhenKidAte = string.Empty, AnySnack = string.Empty, HowKidAte = string.Empty });
            log.Pottys.Add(new Potty() { DiaperCheckTime = string.Empty, DiaperPottyType = string.Empty, PottyTime = string.Empty });
            log.Naps.Add(new Nap() { NapTime = string.Empty });
            log.Activities.Add(new Activity() { Activities = string.Empty, ActivityTime = string.Empty, Mood = string.Empty });
        }

        private void ProcessHelper(KidLog log, Assistant input, bool brandnewReport)
        {
            switch (input.Predicate)
            {
                case Constants.Food:
                    if (brandnewReport)
                    {
                        log.Foods[0].WhenKidAte = input.Time;
                        log.Foods[0].WhatKidAte = input.Object;
                    }
                    else
                    {
                        var food = new Food() { WhenKidAte = input.Time, WhatKidAte = input.Object };
                        log.Foods.Add(food);
                    }
                    break;
                case Constants.Potty:
                    if (brandnewReport)
                    {
                        log.Pottys[0].PottyTime = input.Time;
                        log.Pottys[0].DiaperPottyType = input.Object;
                    }
                    else
                    {
                        var potty = new Potty() { PottyTime = input.Time, DiaperPottyType = input.Object };
                        log.Pottys.Add(potty);
                    }
                    break;
                case Constants.Nap:
                    if (brandnewReport)
                    {
                        log.Naps[0].NapTime = input.Time + " " + input.Object;
                    }
                    else
                    {
                        var nap = new Nap() { NapTime = input.Time + " " + input.Object };
                        log.Naps.Add(nap);
                    }
                    break;
                case Constants.Activity:
                    if (brandnewReport)
                    {
                        log.Activities[0].ActivityTime = input.Time;
                        log.Activities[0].Activities = input.Object;
                    }
                    else
                    {
                        var activity = new Activity() { ActivityTime = input.Time, Activities = input.Object };
                        log.Activities.Add(activity);
                    }
                    break;
                case Constants.Mood:
                    if (brandnewReport)
                    {
                        log.Activities[0].ActivityTime = input.Time;
                        log.Activities[0].Mood = input.Object;
                    }
                    else
                    {
                        var activityMood = new Activity() { ActivityTime = input.Time, Mood = input.Object };
                        log.Activities.Add(activityMood);
                    }
                    break;
                case Constants.Sicks:
                    log.ProblemsConcerns = log.ProblemsConcerns != string.Empty ? log.ProblemsConcerns + "," + input.Object : input.Object;
                    break;
                case Constants.SuppliesSlot:
                    log.SuppliesNeeded = log.SuppliesNeeded != string.Empty ? log.SuppliesNeeded + "," + input.SuppliesNeeded : input.SuppliesNeeded;
                    break;
                case Constants.Misc:
                    log.Comments = log.Comments != string.Empty ? log.Comments + "," + input.Object : input.Object;
                    break;
                default:
                    break;
            }
        }

        public Assistant Parser(Assistant input)
        {
            // Loading english PCFG parser from file
            var lp = LexicalizedParser.loadModel(HttpContext.Current.Server.MapPath("~/englishPCFG.ser.gz"));

            // This option shows loading and using an explicit tokenizer
            var sent2 = input.Message;
            var tokenizerFactory = PTBTokenizer.factory(new CoreLabelTokenFactory(), "");
            var sent2Reader = new StringReader(sent2);
            var rawWords2 = tokenizerFactory.getTokenizer(sent2Reader).tokenize();
            sent2Reader.close();
            var tree2 = lp.apply(rawWords2);

            input.Subject = GetSubject(tree2);
            input.Predicate = GetPredicate(tree2);
            input.Object = GetObject();
            input.Time = CheckPrePosForTime(tree2);
            input.Possession = possessive;
            input.Analyzed = true;

            //give it second chance if everything is null
            if (input.Subject == string.Empty && input.Predicate == string.Empty && input.Object == string.Empty)
            {
                GiveMeSecondChance(input);
            }

            if (input.Subject == string.Empty && input.Predicate != string.Empty && input.Object != string.Empty)
            {
                input.IsCommand = true;
                input.Subject = input.Object;
            }

            if (input.Subject != string.Empty)
                input = GetKidFromSubject(input);

            if (input.Predicate != string.Empty && !input.IsCommand)
            {
                var serv = new DayCareService();
                input.Predicate = serv.GetInstantMessageType(input.Message);
            }

            if ((input.Predicate.ToLower() == Constants.Edit || input.Predicate.ToLower() == Constants.Update) && input.IsCommand)
            {
                input.DestUrl = Constants.EditKidUrlForMobileApp + input.KidId + "/" + input.DayCareId + "/" + string.Empty;
            }
            return input;
        }

        private void GiveMeSecondChance(Assistant input)
        {
            var words = input.Message.Split(' ');
            for (var i = 0; i < words.Length; i++)
            {
                if (words[i].ToLower() == Constants.Edit || words[i].ToLower() == Constants.Update)
                {
                    input.Predicate = words[i];
                    for (int j = i + 1; j < words.Length; j++)
                    {
                        input.Subject = words[j];
                        input = GetKidFromSubject(input);
                        if (input.KidId != 0)
                        {
                            input.Object = input.Subject;
                            input.Subject = string.Empty;
                            break;
                        }
                    }
                }
            }
        }

        private Assistant GetKidFromSubject(Assistant input)
        {
            var repo = new DayCareRepository();
            return repo.GetKidByName(input);
        }

        private string GetSubject(Tree tree)
        {
            if (tree.label().value() == "ROOT")
            {
                if (tree.children().Length == 1 && tree.children()[0].label().value() == "S")
                {
                    var npTree = tree.children()[0];
                    foreach (var child in npTree.children())
                    {
                        //check for NP subtree
                        if (child.label().value() == "NP")
                        {
                            CheckPossession(child.children());
                            return GetFirstNoun(child);
                        }
                    }
                }
            }
            return string.Empty;
        }

        private void CheckPossession(Tree[] tree)
        {
            var posExists = false;
            foreach (var node in tree)
            {
                if (node.label().value() == "NP")
                {
                    foreach (var subNode in node.children())
                    {
                        if (subNode.label().value() == "POS")
                        {
                            posExists = true;
                            break;
                        }
                    }
                }
                else if (node.label().value() == "NN" && posExists)
                {
                    possessive = (possessive != string.Empty ? possessive + " " : "") + node.firstChild().label().value();
                }
                else if (node.label().value() == "NNS" && posExists)
                {
                    possessive = (possessive != string.Empty ? possessive + " " : "") + node.firstChild().label().value();
                }
            }
        }

        private string GetFirstNoun(Tree tree)
        {
            //check for first noun by BFS
            var noun = string.Empty;
            foreach (var subChild in tree.children())
            {
                if (subChild.label().value() == "NN" || subChild.label().value() == "NNP" ||
                    subChild.label().value() == "NNPS" || subChild.label().value() == "NNS")
                {
                    noun = (noun == string.Empty ? "" : noun + " ") + subChild.firstChild().value();
                }
                else if (subChild.label().value() == "JJ" || subChild.label().value() == "VBG")
                {
                    noun = subChild.firstChild().value();
                }
                else if (subChild.label().value() == "NP")
                {
                    return GetFirstNoun(subChild);
                }
            }
            return noun;
        }

        private string GetPredicate(Tree tree)
        {
            if (tree.label().value() == "ROOT")
            {
                if (tree.children().Length == 1 && tree.children()[0].label().value() == "S")
                {
                    var npTree = tree.children()[0];
                    foreach (var child in npTree.children())
                    {
                        //check for VP subtree
                        if (child.label().value() == "VP")
                        {
                            //check for VP sub trees within VP tree
                            var vpSubTree = CheckForVPSubTrees(child.children());
                            if (vpSubTree != null && vpSubTree.label().value() == "VP")
                            {
                                return PredicateHelper(vpSubTree);
                            }
                            else
                                return PredicateHelper(child);
                        }
                    }
                }
            }
            return string.Empty;
        }

        private string PredicateHelper(Tree tree)
        {
            VPTree = tree;
            foreach (var subChild in tree.children())
            {
                if (subChild.label().value() == "VB" || subChild.label().value() == "VBD" ||
                    subChild.label().value() == "VBG" || subChild.label().value() == "VBN" || subChild.label().value() == "VBP"
                    || subChild.label().value() == "VBZ")
                {
                    return subChild.firstChild().value();
                }
            }
            return string.Empty;
        }

        private Tree CheckForVPSubTrees(Tree[] children)
        {
            foreach (var child in children)
            {
                //check for VP subtree
                if (child.label().value() == "VP")
                {
                    return child;
                }
            }
            return null;
        }

        private string GetObject()
        {
            //check for VP subtree siblings for object
            if (VPTree != null)
            {
                foreach (var child in VPTree.children())
                {
                    if (child.label().value() == "PP")
                    {
                        foreach (var subChild in child.children())
                        {
                            if (subChild.label().value() == "NP")
                            {
                                return GetFirstNoun(subChild);
                            }
                        }
                    }
                    else if (child.label().value() == "NP")
                    {
                        return GetFirstNoun(child);
                    }
                    else if (child.label().value() == "ADJP")
                    {
                        foreach (var subChild in child.children())
                        {
                            if (subChild.label().value() == "JJ" || subChild.label().value() == "JJR" ||
                                subChild.label().value() == "JJS")
                            {
                                return subChild.firstChild().value();
                            }
                        }
                    }
                    else if (child.label().value() == "VP")
                    {
                        VPTree = child;
                        return GetObject();
                    }
                    else if (child.label().value() == "S")
                    {
                        VPTree = child.firstChild();
                        return GetObject();
                    }
                    else if (child.label().value() == "SBAR")
                    {
                        //check for question within the sentence
                        foreach (var quesChild in child.children())
                        {
                            if (quesChild.label().value() == "S")
                            {
                                VPTree = quesChild.firstChild();
                                return GetObject();
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }

        private string CheckPrePosForTime(Tree tree)
        {
            Queue<Tree> q = new Queue<Tree>();
            Tree ppTree = null;
            q.Enqueue(tree);
            while (q.Count > 0)
            {
                Tree current = q.Dequeue();
                if (current == null)
                    continue;
                if (current.label().value() == "PP")
                {
                    ppTree = current;
                    break;
                }
                foreach (var item in current.children())
                {
                    if (item.label().value() == "PP")
                    {
                        ppTree = item;
                        break;
                    }
                    q.Enqueue(item);
                }
                if (ppTree != null)
                    break;
            }
            if (ppTree != null)
            {
                var timeExists = false;
                var time = string.Empty;
                foreach (var node in ppTree.children())
                {
                    if (node.label().value() == "IN" &&
                        (node.firstChild().label().value().ToLower() == "at" || node.firstChild().label().value().ToLower() == "for"))
                    {
                        timeExists = true;
                    }
                    else if (node.label().value() == "NP" && timeExists)
                    {
                        time = PrePosForTimeHelper(node);
                    }
                }
                return time;
            }
            return string.Empty;
        }

        private string PrePosForTimeHelper(Tree tree)
        {
            var time = string.Empty;
            //get the cardinal number
            foreach (var item in tree.children())
            {
                if (item.label().value() == "CD")
                    time = item.firstChild().label().value();
                else if (item.label().value() == "NN" || item.label().value() == "NNP"
                    || item.label().value() == "NNPS" || item.label().value() == "NNS")
                {
                    time = (time != string.Empty ? time + " " : "") + item.firstChild().label().value();
                }
                else if (item.label().value() == "NP")
                {
                    return PrePosForTimeHelper(item);
                }
            }
            return time;
        }
    }
}
