document.addEventListener("DOMContentLoaded", function () {

    // 1. Get Data safely (Prevent crash if data is missing)
    // 'dbProgramData' is defined in the Razor View
    const programData = (typeof dbProgramData !== 'undefined') ? dbProgramData : {};

    // 2. Dropdown Logic
    const programSelect = document.getElementById("programSelect");
    const programCountDisplay = document.getElementById("programCountDisplay");

    function updateProgramCount() {
        if (!programSelect) return;

        const selected = programSelect.value;
        const count = programData[selected] || 0;
        programCountDisplay.innerText = count;
    }

    if (programSelect) {
        programSelect.addEventListener("change", updateProgramCount);
        // Trigger once on load to show initial value
        updateProgramCount();
    }

    // 3. Bar Chart Logic
    const ctx = document.getElementById('studentsBarChart');
    if (ctx) {
        // Extract keys (Program Names) and values (Student Counts)
        const labels = Object.keys(programData);
        const dataValues = Object.values(programData);

        new Chart(ctx.getContext('2d'), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Enrolled Students',
                    data: dataValues,
                    backgroundColor: 'rgba(46, 125, 50, 0.7)', // Green
                    borderColor: 'rgba(46, 125, 50, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: { stepSize: 1 } // Ensure whole numbers
                    }
                },
                plugins: { legend: { display: false } }
            }
        });
    }
});