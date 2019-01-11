$("#txtSearch").keypress(function (e) {
	if (e.keyCode == 13) {
		$(".searchBtn").click();
	}
});

$(".searchBtn").click(function () {
	var searchText = $("#txtSearch").val().trim();
	if (searchText.length == 0) {
		alert("검색 할 채널/프로그램명을 입력하여 주세요.");
		return false;
	}

	$("#pnlSearchResult").empty();
	$("#pnlSearchResult").data("status", "searching");
	var searchMsgHtml = "검색 중입니다. 잠시만 기다려주세요.<a href='javascript:void(0);' onclick='searchCancel()'>취소</a>";
	$("#pnlSearchResult").html(searchMsgHtml);

	if ($("#pnlSearchResult").css("display") == "none")
		$("#pnlSearchResult").show();

	$.post("/TvProgram/SearchProgram", { searchText: searchText }, function (rtn) {
		if (rtn.Result == "OK") {
			$("#pnlSearchResult").empty();
			// 여기서 태깅 채널 버튼 바인딩 처리
			// 문제발생!!
			// 결과 리스트 모델을 OllehProram으로 해버렸더니 결과로서 적합하지 않다
			// 찾기 결과 모델을 별도로 만들어야 하며 채널 버튼으로 보여줄껀지 아니면
			// 해당 프로그램의 상세 정보를 보여줄껀지 결정해야한다. 니미...
			// ------------------------------------------------------------------------------------------------------
			// 지금으로썬 해당 프로그램이 있는 채널 버튼을 보여주는게 맞는것 같다..

			var resultCloseHtml = "<a href='javascript:void(0);' onclick='searchCancel()'>닫기</a><br />";
			$("#pnlSearchResult").append(resultCloseHtml);
			$.each(rtn.FoundList, function (idx, ch) {
				var displayName = ch.chName + "(" + ch.chNum + ")";
				var btn = "<button type=\"button\" class=\"btn btn-info\" channel-id=\"" + ch.chNum + "\" onclick=\"ShowModal('" + ch.chNum + "', '" + ch.chName + "')\" style=\"border-radius: 0 !important;\">" + displayName + "</button>";
				$("#pnlSearchResult").append(btn);
			});

		} else if (rtn.Result == "Empty") {
			$("#pnlSearchResult").empty();
			var resultMsgHtml = "검색 결과가 없습니다.<a href='javascript:void(0);' onclick='searchCancel()'>닫기</a>";
			$("#pnlSearchResult").html(resultMsgHtml);
		}
		$("#pnlSearchResult").data("status", "complete");
	});
});

var HasChannel = function (channels, chName) {
	var blRtn = false;
	var defGW = channels;
	defGW.forEach(function (val, idx) {
		chName = chName.toLowerCase();
		if (chName.indexOf(val) != -1) {
			blRtn = true;
		}
	});
	return blRtn;
}

var ShowModal = function (chNum, chName) {
	$.get("/TvProgram/GetOllehProgramList?chNum=" + chNum + "&chName=" + chName, "", function (rtn) {
		$("#programmList").empty();
		var modalTitle = chName + "(" + chNum + ")";
		$(".modal-title").text(modalTitle);	// 모달 타이틀
		if (rtn != null) {
			var half = Math.ceil(rtn.length / 2) - 1;
			var appendTarget = "programmList";
			$.each(rtn, function (idx, prog) {
				var head = "<b>" + prog.time + "</b> " + prog.title + "(" + prog.ageLimit + ")";
				var ageLimit = prog.ageLimit;
				var row = "<li class=\"list-group-item" + (prog.BeOnAir ? " active" : "") + "\">"
				+ "<table>"
				+ "<tr><td style=\"width:90%\">"
				+ "	<h4 class=\"list-group-item-heading\">" + head + "</h4>"
				+ "	<p class=\"list-group-item-text\">" + ageLimit + "</p>"
				+ "</td><td class=\"text-right text-nowrap\" style=\"width:10%\">"
				+ "    <button type=\"button\" onclick=\"alarmToggle(this, '" + prog.Title + "', '" + prog.Start + "')\" class=\"btn btn-default\">"
				+ "		<span class=\"glyphicon glyphicon-time\"></span> Alarm"
				+ "	</button>"
				+ "</td></tr>"
				+ "</table>"
				+ "</li>";

				if (idx == half) {
					var adsense = "<li class=\"list-group-item\">"
					+ "<!-- 편성표2 -->"
					+ "<ins class=\"adsbygoogle\""
					+ "	style=\"display:block\""
					+ "	data-ad-client=\"ca-pub-9553320581194506\""
					+ "	data-ad-slot=\"5520393603\""
					+ "	data-ad-format=\"auto\"></ins>"
					+ "</li>";
					row = adsense + row;
				}

				$("#" + appendTarget).append(row);
			});
		}

		$("#layerpop").modal();
	});
}

//----------------------------------------------------------------------------------------------------------------
// alarmToggle
//----------------------------------------------------------------------------------------------------------------
var alarmToggle = function (btn, title, time) {
	btn = $(btn);
	var btnStatus = "on";
	if (btn.hasClass("btn-default")) {
		btn.removeClass("btn-default");
		btn.addClass("btn-success");
	} else if (btn.hasClass("btn-success")) {
		btn.removeClass("btn-success");
		btn.addClass("btn-default");
		btnStatus = "off";
	}

	if (IsWebView) {
		var setStatusMsg = "";
		if (btnStatus == "on") {
			setStatusMsg = time + "에 알람이 설정 되었습니다.";
		} else if (btnStatus == "off") {
			setStatusMsg = "해당 프로그램의 알람이 설정 해제 되었습니다.";
		}

		//NativeCall.showToast(setStatusMsg);
		// 채널명, 프로그램명, 시간
		var chName = $(".modal-title").text();
			
		NativeCall.setAlarm(chName, title, time);
	}
}

var IsWebView = function () {
	var userAgent = navigator.userAgent;
	return (userAgent.indexOf("Android") != -1 ? true : false);
}

$(document).ready(function () {
	// 프로그램 리스트를 띄울 모달 창이 완전히 열렸을때 이벤트를 잡아 구글 광고 초기화
	$("#layerpop").on("shown.bs.modal", function () {
		(adsbygoogle = window.adsbygoogle || []).push({});
	});

	// 이 부분에서 GetOllehTabList를 호출하여 큰 분류 탭 버튼을 바인딩하고
	// 각 탭에 할당되어 있는 div에 편성표를 바인딩하는 작업을 진행하자 [2017-03-31]
	$.get("/TvProgram/GetOllehMainClassifies", "", function (rtn) {
		if (rtn != null) {
			$("#slickMenu").empty();
			$.each(rtn, function (idx, classify) {
				var anchor = "<a href=\"#\" id=\"tab" + String(idx) + "\" data-toggle=\"tab\">" + classify + "</a>";
				$("#slickMenu").append(anchor);
			});

			// 상단 대분류 채널 Slick 로테이션 메뉴 설정
			$("#slickMenu").slick({
				dots: true,
				infinite: false,
				arrows: false,
				slidesToShow: 3,
				slidesToScroll: 3
			});

			// 대분류 채널을 클릭했을때
			$("#slickMenu a").click(function () {
				$(".tab-content div").each(function (idx, area) {
					$(area).hide();
				});
				var id = $(this).attr("id");

				// 각 분류 별 편성표 바인딩
				var tabIdx = id.replace("tab", "");
				var tabAreaId = id + "Area";
				$.get("/TvProgram/GetOllehChannelList?idx=" + tabIdx, "", function (rtn) {
					var tabArea = "<div id=\"" + tabAreaId + "\" class=\"tab-pane\">";
					tabArea += "<div class=\"row\">";
					if (rtn != null) {
						$("#" + tabAreaId).remove();
						$.each(rtn, function (idx, ch) {
							var btn = "<span class=\"badge\" channel-id=\"" + ch.chNum + "\" onclick=\"ShowModal('" + ch.chNum + "', '" + ch.chName + "')\" style=\"cursor:pointer\">" + ch.chName + "</span>";
							tabArea += btn;
						});
						tabArea += "</div></div>";

					} else {
						tabArea += "	<h3>편성표 업데이트 중입니다.</h3>"
						tabArea += "</div>";
					}

					$(".tab-content").append(tabArea);
					$(".tab-pane").removeClass("active").removeClass("show");
					$("#" + tabAreaId).show().addClass("active").addClass("show");
				});

				// Slick 메뉴에 효과 넣어주기
				$("#slickMenu div").removeClass("chn-1st-active");
				$(this).parent().addClass("chn-1st-active");

				$("#slickMenu div").removeClass("slick-current");
				$(this).parent().parent().addClass("slick-current");
			});

			$("#tab0").click();
		}
	});

	// 검색 버튼 누를때
	$("#btnSearch").click(function () {
		$(".dropdown-menu").show();
	});

	// 검색 관련
	$("#txtSearch").keypress(function (e) {
		if (e.keyCode == 13) {
			$(".searchBtn").click();
		}
	});

	$(".searchBtn").unbind();
	$(".searchBtn").click(function () {
		event.preventDefault();

		var searchText = $("#txtSearch").val().trim();
		if (searchText.length == 0) {
			alert("검색 할 채널/프로그램명을 입력하여 주세요.");
			return false;
		}

		$("#pnlSearchResult > #resultMessage").empty();
		$("#pnlSearchResult").data("status", "searching");
		var searchMsgHtml = "검색 중입니다. 잠시만 기다려주세요.<a href='javascript:void(0);' onclick='searchCancel()'>취소</a>";
		$("#pnlSearchResult > #resultMessage").html(searchMsgHtml);

		if ($("#pnlSearchResult").css("display") == "none")
			$("#pnlSearchResult").fadeIn(300);

		$.post("/TvProgram/SearchProgram", { searchText: searchText }, function (rtn) {
			if (rtn.Result == "OK") {
				$("#pnlSearchResult > #resultMessage").empty();

				var resultCloseHtml = "<a href='javascript:void(0);' onclick='searchCancel()'>닫기</a><br />";
				$("#pnlSearchResult > #resultMessage").append(resultCloseHtml);
				$.each(rtn.FoundList, function (idx, ch) {
					var displayName = ch.chName + "(" + ch.chNum + ")";
					var btn = "<button type=\"button\" class=\"btn btn-info\" channel-id=\"" + ch.chNum + "\" onclick=\"ShowModal('" + ch.chNum + "', '" + ch.chName + "')\" style=\"border-radius: 0 !important;\">" + displayName + "</button>";
					$("#pnlSearchResult > #resultMessage").append(btn);
				});

			} else if (rtn.Result == "Empty") {
				$("#pnlSearchResult > #resultMessage").empty();
				var resultMsgHtml = "검색 결과가 없습니다. ->&nbsp;<a href='javascript:void(0);' onclick='searchCancel()'>닫기</a>";
				$("#pnlSearchResult > #resultMessage").html(resultMsgHtml);
			}
			$("#pnlSearchResult").data("status", "complete");
		});
	});
});

function searchCancel() {
	$("#pnlSearchResult").fadeOut(300);
}