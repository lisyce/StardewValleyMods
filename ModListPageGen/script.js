const checkbox = document.getElementById("toggle");
const detailed = document.getElementById("detail-list");
const simple = document.getElementById("simple-list");
checkbox.addEventListener("change", () => {
    if (checkbox.checked) {
        simple.style.display = "block";
        detailed.style.display = "none";
    } else {
        simple.style.display = "none";
        detailed.style.display = "grid";
    }
})
for (let box of document.querySelectorAll(".category-box, .dep-box")) {
    box.addEventListener("change", () => {
        for (let el of document.getElementsByClassName(box.value)) {
            if (visible(el)) {
                el.style.removeProperty("display");
            } else {
                el.style.display = "none";
            }
        }
    })
}
function visible(el) {
    // chekc category filter
    let foundCategoryOn = false;
    for (let box of document.getElementsByClassName("category-box")) {
        if (box.checked && el.classList.contains(box.value)) {
            foundCategoryOn = true;
            break;
        }
    }
    if (!foundCategoryOn) return false;
    // check dep filters (at least one gotta be on)
    for (let box of document.getElementsByClassName("dep-box")) {
        if (box.checked && el.classList.contains(box.value)) {
            return true;
        }
    }
    return false;
}
document.getElementById("category-enable").addEventListener("click", () => {
    for (let box of document.getElementsByClassName("category-box")) {
        box.checked = true
        box.dispatchEvent(new Event("change"))
    }
})
document.getElementById("dep-enable").addEventListener("click", () => {
    for (let box of document.getElementsByClassName("dep-box")) {
        box.checked = true
        box.dispatchEvent(new Event("change"))
    }
})
document.getElementById("category-disable").addEventListener("click", () => {
    for (let box of document.getElementsByClassName("category-box")) {
        box.checked = false
        box.dispatchEvent(new Event("change"))
    }
})
document.getElementById("dep-disable").addEventListener("click", () => {
    for (let box of document.getElementsByClassName("dep-box")) {
        box.checked = false
        box.dispatchEvent(new Event("change"))
    }
})