
const BudgetPlanningSetup = (function () {
    // Private variables
    let currentStep = 1;
    let selectedYearId = null;
    let selectedYearName = "";
    let organizationsImported = false;

    // Step definitions
    const steps = [
        { id: 'year', title: 'Select Year', icon: 'calendar' },
        { id: 'orgs', title: 'Organizations', icon: 'building' },
        { id: 'accounts', title: 'Accounts', icon: 'database' },
        { id: 'budget-types', title: 'Budget Types', icon: 'chart-pie' },
        { id: 'assign-orgs', title: 'Assign Orgs', icon: 'folders' },
        { id: 'submissions', title: 'Setup Submissions', icon: 'file-spreadsheet' },
        { id: 'account-setup', title: 'Setup Accounts', icon: 'database' }
    ];

    // Initialize
    const init = function () {
        renderProgressSteps();

        // Check current step from server
        $.ajax({
            url: '/BudgetPlanningSetup/GetCurrentStep',
            method: 'GET',
            success: function (response) {
                currentStep = response.currentStep;
                updateUI();
            },
            error: function () {
                // Default to step 1 if there's an error
                currentStep = 1;
                updateUI();
            }
        });

        // Attach event handlers
        $('#next-button').click(handleNextClick);
        $('#prev-button').click(handlePrevClick);
        $('#fetch-button').click(handleFetchClick);

        // Load initial data for step 1
        loadFinancialYears();
    };

    // Render progress steps
    const renderProgressSteps = function () {
        const container = $('#progress-steps-container');
        let html = '';

        steps.forEach((step, index) => {
            html += `
            <div class="position-relative d-flex flex-column align-items-center step-indicator" data-step="${index + 1}">
                <div class="d-flex align-items-center justify-content-center rounded-circle bg-white border border-2 border-secondary-subtle text-secondary step-circle" style="width: 40px; height: 40px; z-index: 1;">
                    <span class="step-number fw-medium">${index + 1}</span>
                    <i class="ti ti-check step-check d-none"></i>
                </div>
                <span class="mt-2 small fw-medium text-secondary step-title">${step.title}</span>
            </div>
        `;
        });

        container.html(html);
    };

    // Update UI based on current step
    const updateUI = function () {
        // Update progress indicator
        $('.step-indicator').each(function () {
            const stepNum = parseInt($(this).data('step'));
            const circle = $(this).find('.step-circle');
            const number = $(this).find('.step-number');
            const check = $(this).find('.step-check');
            const title = $(this).find('.step-title');

            if (stepNum < currentStep) {
                // Completed step
                circle.removeClass('bg-white border-secondary-subtle text-secondary')
                    .removeClass('bg-primary text-white')
                    .addClass('bg-success text-white border-success');
                number.addClass('d-none');
                check.removeClass('d-none');
                title.removeClass('text-secondary text-primary')
                    .addClass('text-success');
            } else if (stepNum === currentStep) {
                // Current step
                circle.removeClass('bg-white border-secondary-subtle text-secondary')
                    .removeClass('bg-success text-white border-success')
                    .addClass('bg-primary text-white border-primary');
                number.removeClass('d-none');
                check.addClass('d-none');
                title.removeClass('text-secondary text-success')
                    .addClass('text-primary');
            } else {
                // Future step
                circle.removeClass('bg-primary text-white border-primary')
                    .removeClass('bg-success text-white border-success')
                    .addClass('bg-white border-secondary-subtle text-secondary');
                number.removeClass('d-none');
                check.addClass('d-none');
                title.removeClass('text-primary text-success')
                    .addClass('text-secondary');
            }
        });

        // Show/hide step content
        $('.step-content').addClass('d-none');
        $(`#step-${currentStep}-content`).removeClass('d-none');

        // Update navigation buttons
        updateNavigationButtons();

        // Update header subtitle
        updateHeaderSubtitle();
    };

    // Update navigation buttons state
    const updateNavigationButtons = function () {
        // Previous button
        if (currentStep > 1) {
            $('#prev-button').removeClass('d-none');
        } else {
            $('#prev-button').addClass('d-none');
        }

        // Next button
        const nextButton = $('#next-button');

        // Reset next button
        nextButton.prop('disabled', true)
            .removeClass('btn-primary')
            .addClass('btn-secondary opacity-50');

        // Enable based on current step completion
        switch (currentStep) {
            case 1:
                if (selectedYearId !== null) {
                    nextButton.prop('disabled', false)
                        .removeClass('btn-secondary opacity-50')
                        .addClass('btn-primary');
                }
                break;
            case 2:
                if (organizationsImported) {
                    nextButton.prop('disabled', false)
                        .removeClass('btn-secondary opacity-50')
                        .addClass('btn-primary');
                }
                break;
            // Additional cases will be added for future steps
        }
    };

    // Update header subtitle based on current step
    const updateHeaderSubtitle = function () {
        let subtitle = '';

        switch (currentStep) {
            case 1:
                subtitle = 'Select a financial year in planning status to begin';
                break;
            case 2:
                subtitle = selectedYearName ?
                    `Import organizations for ${selectedYearName}` :
                    'Import organizations from the external system';
                break;
            // Additional cases will be added for future steps
        }

        $('#header-subtitle').text(subtitle);
    };

    // Load financial years
    const loadFinancialYears = function () {
        $.ajax({
            url: '/BudgetPlanningSetup/GetPlanningYears',
            method: 'GET',
            success: function (data) {
                const container = $('#years-container');
                container.empty();

                if (data.length === 0) {
                    container.html(`
                        <div class="col-12">
                            <div class="alert alert-warning" role="alert">
                                <div class="d-flex align-items-center mb-2">
                                    <i class="ti ti-alert-triangle me-2"></i>
                                    <span>No financial years in planning status are available. Please create the Planning Financial Year first.</span>
                                </div>
                                <div class="mt-3">
                                    <a href="/FinancialYear/Create" class="btn btn-warning">
                                        <i class="ti ti-plus me-1"></i> Create New Financial Year
                                    </a>
                                </div>
                            </div>
                        </div>
                    `);
                    return;
                }

                let html = '';
                data.forEach(year => {
                    html += `
                        <div class="col-sm-6 col-md-4 col-lg-3">
                            <div class="card border year-card ${selectedYearId === year.id ? 'border-warning' : ''}" 
                                 data-year-id="${year.id}" 
                                 data-year-name="${year.name}">
                                <div class="card-body text-center">
                                    <h5 class="card-title">${year.name}</h5>
                                    <p class="card-text text-muted small">
                                        ${new Date(year.startDate).toLocaleDateString()} - 
                                        ${new Date(year.endDate).toLocaleDateString()}
                                    </p>
                                </div>
                            </div>
                        </div>
                    `;
                });

                container.html(html);

                // Attach click event to year cards
                $('.year-card').click(function () {
                    $('.year-card').removeClass('border-warning');
                    $(this).addClass('border-warning');

                    selectedYearId = parseInt($(this).data('year-id'));
                    selectedYearName = $(this).data('year-name');

                    updateNavigationButtons();
                });
            },
            error: function () {
                $('#years-container').html(`
                    <div class="col-12">
                        <div class="alert alert-danger" role="alert">
                            <i class="ti ti-alert-circle me-2"></i>
                            Error loading financial years. Please try again.
                        </div>
                    </div>
                `);
            }
        });
    };

    // Handle fetch button click
    const handleFetchClick = function () {
        // Show loading indicator
        $('#fetch-button').prop('disabled', true);
        $('#loading-indicator').removeClass('d-none');
        $('#organizations-container').addClass('d-none');
        $('#error-container').addClass('d-none');

        // Make the AJAX request to import organizations
        $.ajax({
            url: '/BudgetPlanningSetup/ImportOrganizations',
            method: 'POST',
            success: function (response) {
                // Hide loading indicator
                $('#loading-indicator').addClass('d-none');

                if (response.success) {
                    // Fetch and display the imported organizations
                    fetchOrganizations();

                    // Mark as imported
                    organizationsImported = true;

                    // Update fetch button
                    $('#fetch-button').text('Organizations Imported')
                        .removeClass('btn-primary')
                        .addClass('btn-success');

                    // Update navigation buttons
                    updateNavigationButtons();

                    // Show success message
                    showAlert('success', `${response.message}`);
                } else {
                    // Show error message
                    $('#error-message').text(response.message);
                    $('#error-container').removeClass('d-none');

                    // Re-enable fetch button
                    $('#fetch-button').prop('disabled', false);
                }
            },
            error: function () {
                // Hide loading indicator
                $('#loading-indicator').addClass('d-none');

                // Show error message
                $('#error-message').text('Failed to import organizations. Please try again.');
                $('#error-container').removeClass('d-none');

                // Re-enable fetch button
                $('#fetch-button').prop('disabled', false);
            }
        });
    };

    // Fetch and display organizations
    const fetchOrganizations = function () {
        $.ajax({
            url: '/BudgetPlanningSetup/GetOrganizations',
            method: 'GET',
            success: function (data) {
                const tableBody = $('#organizations-table-body');
                tableBody.empty();

                if (data.length === 0) {
                    tableBody.html('<tr><td colspan="3" class="text-center">No organizations found.</td></tr>');
                } else {
                    data.forEach(org => {
                        tableBody.append(`
                            <tr>
                                 <td>${org.costCenter || ''}</td>
                                 <td>${org.name || ''}</td>
                                 <td>${org.isActive ? 'Active' : 'Inactive'}</td>
                            </tr>
                        `);
                    });
                }

                // Show the organizations container
                $('#organizations-container').removeClass('d-none');
            },
            error: function () {
                // Show error message
                $('#error-message').text('Failed to retrieve organizations. Please try again.');
                $('#error-container').removeClass('d-none');
            }
        });
    };

    // Handle next button click
    // Handle next button click
    const handleNextClick = function () {
        // Dismiss any existing alerts when moving to next step
        $('#alert-container .alert').alert('close');

        if (currentStep === 1) {
            // When moving from step 1 to 2, save the selected year to the server
            $.ajax({
                url: '/BudgetPlanningSetup/SelectFinancialYear',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ financialYearId: selectedYearId }),
                success: function (response) {
                    if (response.success) {
                        // Move to next step
                        currentStep++;
                        updateUI();
                    } else {
                        // Show error message
                        showAlert('danger', response.message);
                    }
                },
                error: function () {
                    showAlert('danger', 'Failed to select financial year. Please try again.');
                }
            });
        } else {
            // For other steps, simply move to the next step
            currentStep++;
            updateUI();
        }
    };

    // Handle previous button click
    const handlePrevClick = function () {
        if (currentStep > 1) {
            currentStep--;
            updateUI();
        }
    };

    // Show alert message
    const showAlert = function (type, message) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <i class="ti ti-${type === 'success' ? 'check' : 'alert-circle'} me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        $('#alert-container').html(alertHtml);
        
    };

    // Public API
    return {
        init: init
    };
})();

// Initialize the SPA when the document is ready
$(document).ready(function () {
    BudgetPlanningSetup.init();
});