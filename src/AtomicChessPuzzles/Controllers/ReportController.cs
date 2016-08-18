﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using AtomicChessPuzzles.DbRepositories;
using Microsoft.AspNet.Http;
using AtomicChessPuzzles.Attributes;
using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.Controllers
{
    public class ReportController : RestrictedController
    {
        IReportRepository reportRepository;

        public static readonly string[] ValidCommentReportReasons = new string[] { "Offensive", "Spam", "Off-topic", "Other" };
        public static readonly string[] ValidPuzzleReportReasons = new string[] { "Inaccurate", "Too many options", "Missing answer" };

        static readonly Dictionary<string, string[]> validReasonsForType = new Dictionary<string, string[]>()
        {
            { "Comment", ValidCommentReportReasons },
            { "Puzzle", ValidPuzzleReportReasons }
        };

        public ReportController(IReportRepository _reportRepository, IUserRepository _userRepository) : base(_userRepository)
        {
            reportRepository = _reportRepository;
        }

        [Route("/Report/List/All")]
        [Restricted(true, UserRole.COMMENT_MODERATOR, UserRole.PUZZLE_EDITOR)]
        public IActionResult ListAll()
        {
            User user = userRepository.FindById(HttpContext.Session.GetInt32("userid").Value);
            List<string> roles = user.Roles;
            List<string> types = new List<string>();
            if (UserRole.HasAtLeastThePrivilegesOf(roles, UserRole.COMMENT_MODERATOR))
            {
                types.Add("Comment");
            }
            if (UserRole.HasAtLeastThePrivilegesOf(roles, UserRole.PUZZLE_EDITOR))
            {
                types.Add("Puzzle");
            }
            return View("List", reportRepository.GetByTypes(types));
        }

        [Route("/Report/List/Comments")]
        [Restricted(true, UserRole.COMMENT_MODERATOR)]
        public IActionResult ListCommentReports()
        {
            return View("List", reportRepository.GetByType("Comment"));
        }

        [Route("/Report/List/Puzzles")]
        [Restricted(true, UserRole.PUZZLE_EDITOR)]
        public IActionResult ListPuzzleReports()
        {
            return View("List", reportRepository.GetByType("Puzzle"));
        }

        [HttpPost]
        [Route("/Report/Submit/{type}")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult SubmitReport(string type, string item, string reason, string reasonExplanation)
        {
            string[] validTypes = new string[] { "Comment", "Puzzle" };
            if (!validTypes.Contains(type))
            {
                return Json(new { success = false, error = "Unknown report type." });
            }
            if (!validReasonsForType[type].Contains(reason))
            {
                return Json(new { success = false, error = "Invalid reason" });
            }
            Report report = new Report(Guid.NewGuid().ToString(), type, HttpContext.Session.GetInt32("userid").Value, item, reason, reasonExplanation, false, null);
            if (reportRepository.Add(report))
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Reporting failed." });
            }
        }

        [HttpGet]
        [Route("/Report/Dialog/Comment")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult CommentReportDialog()
        {
            return View();
        }

        [HttpGet]
        [Route("/Report/Dialog/Puzzle")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult PuzzleReportDialog()
        {
            return View();
        }
    }
}
