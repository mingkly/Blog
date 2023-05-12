async function Examine() {
    var action = $("#examineAction").val()
    var id = $("#examineId").val()
    var text = $("#examineText").val()
    var res = await fetch("api/Articles/ExamineArticle", {
        method: 'POST',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
            'content-type': 'application/json',
            'accept': 'application/json'
        },
        body: JSON.stringify({
            Action: action,
            Id: id,
            Text: text
        })
    });
    if (res.ok) {
        var json = await res.json();
        console.log(json);
        if (json.id == "suc") {
            if (json.msg == "0") {
                $("#" + id + "state").html("通过");
            }
            else if (json.msg == "1") {
                $("#" + id + "state").html("拒绝");
            }
            else if (json.msg == "2") {
                $("#" + id + "tr").remove();
            }

        }
        else {
            alert(json.msg);
        }
    }
}

async function deleteComment(id, comment) {
    var res = await fetch("api/articles/deletecomment/" + id, {
        method: 'GET',
        headers: {
            'accept': 'application/json'
        }
    })
    if (res.ok) {
        var json = await res.json();
        if (json.code == -1) {
            $(comment).parent().parent().remove();
        }
    }
}
async function baned(btn, name) {
    var res = await fetch("api/User/ExamineUser", {
        method: 'POST',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
            'content-type': 'application/json',
            'accept': 'application/json'
        },
        body: JSON.stringify({
            Name: name,
            Action: 2,
            Text: "",
        })
    })
    if (res.ok) {
        var json = await res.json();
        if (json.id == 0) {
            btn.innerHTML = '已封禁';
        }
        else if (json.id < 0) {
            alert("无此人");
        }
        else {
            alert("失败");
        }
    }
}