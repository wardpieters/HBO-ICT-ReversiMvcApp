var strength = {
    0: "Dramatisch ☹",
    1: "Slecht ☹",
    2: "Zwak ☹",
    3: "Redelijk ☺",
    4: "Sterk ☻"
}

var password = document.getElementsByClassName('password-control')[0];
var meter = document.getElementById('password-strength-meter');
var text = document.getElementById('password-strength-text');

password.addEventListener('input', function()
{
    var val = password.value;
    var result = zxcvbn(val);

    // Update the password strength meter
    meter.value = result.score;
    console.log(meter.value, result.score)

    // Update the text indicator
    if(val !== "") {
        text.innerHTML = "<strong>" + strength[result.score] + "</strong>" + "<span class='feedback d-block'>" + result.feedback.warning + " " + result.feedback.suggestions + "</span";
    }
    else {
        text.innerHTML = "";
    }
});