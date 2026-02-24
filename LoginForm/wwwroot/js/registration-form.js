$(document).ready(function () {
    const form = document.getElementById('registrationForm');

    // Password strength checker
    $('#password').on('input', function () {
        const password = $(this).val();
        const strengthIndicator = $('#passwordStrength');

        if (password.length < 8) {
            strengthIndicator.removeClass('weak medium strong').addClass('weak').text('❌ Weak: At least 8 characters required');
            return;
        }

        const hasUpperCase = /[A-Z]/.test(password);
        const hasLowerCase = /[a-z]/.test(password);
        const hasNumber = /\d/.test(password);
        const hasSpecialChar = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password);

        const strength = [hasUpperCase, hasLowerCase, hasNumber, hasSpecialChar].filter(Boolean).length;

        strengthIndicator.removeClass('weak medium strong');

        if (strength < 3) {
            strengthIndicator.addClass('weak').text('❌ Weak: Add more variety to your password');
        } else if (strength === 3) {
            strengthIndicator.addClass('medium').text('⚠️ Medium: Password is acceptable');
        } else {
            strengthIndicator.addClass('strong').text('✓ Strong: Excellent password strength');
        }
    });

    // Name length validation (max 50 characters)
    const nameFields = ['firstName', 'middleName', 'lastName'];
    nameFields.forEach(fieldId => {
        $(`#${fieldId}`).on('input', function () {
            const fieldValue = $(this).val();
            const fieldLabel = $(this).prev('.form-label').text().replace(/\s*\*$/, '').replace(/\s*Optional$/, '').trim();

            if (fieldValue.length > 50) {
                alert(`${fieldLabel} cannot exceed 50 characters. Current length: ${fieldValue.length}`);
            }
        });
    });

    // DOB validation (minimum 18 years old)
    $('#dob').on('change', function () {
        const dobValue = $(this).val();
        if (dobValue) {
            const dob = new Date(dobValue);
            const today = new Date();

            // Calculate age
            let age = today.getFullYear() - dob.getFullYear();
            const monthDiff = today.getMonth() - dob.getMonth();

            if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) {
                age--;
            }

            if (age < 18) {
                alert(`You must be at least 18 years old to register. Current age: ${age} years`);
                $(this).val('');
                $(this).focus();
            }
        }
    });

    // Confirm password validation
    $('#confirmPassword').on('input', function () {
        const password = $('#password').val();
        const confirmPassword = $(this).val();
        const feedback = $('#confirmPasswordFeedback');

        if (confirmPassword === '' || password === confirmPassword) {
            $(this).removeClass('is-invalid');
            feedback.removeClass('show');
        } else {
            $(this).addClass('is-invalid');
            feedback.addClass('show');
        }
    });

    // Sync alternate address with permanent address
    $('#sameAsPermenant').on('change', function () {
        if ($(this).is(':checked')) {
            // Copy permanent address to alternate address and lock it (readonly)
            $('#alternateAddress').val($('#permanentAddress').val());
            $('#alternateAddress').prop('readonly', true).css('background-color', '#f5f5f5');
        } else {
            // Unlock alternate address and clear it
            $('#alternateAddress').prop('readonly', false).css('background-color', '');
            $('#alternateAddress').val('');
        }
    });

    // Sync permanent address changes to alternate address if checkbox is checked
    $('#permanentAddress').on('input change', function () {
        if ($('#sameAsPermenant').is(':checked')) {
            $('#alternateAddress').val($(this).val());
        }
    });

    // File upload handling for marksheets
    setupFileUpload('class12Area', 'class12Marksheet', 'class12Name', null, ['pdf']);
    setupFileUpload('class10Area', 'class10Marksheet', 'class10Name', null, ['pdf']);
    setupFileUpload('profileUploadArea', 'profileUpload', 'profileUploadName', 'profileImage', ['jpg', 'jpeg', 'png']);

    function setupFileUpload(areaId, inputId, nameId, imageId = null, allowedExtensions = []) {
        const area = $(`#${areaId}`);
        const input = $(`#${inputId}`);
        const nameDisplay = $(`#${nameId}`);

        // Click to upload - using both jQuery and vanilla JS
        area.on('click', function (e) {
            // Don't trigger if clicking on the input itself
            if (e.target.tagName !== 'INPUT') {
                input[0].click();
            }
        });

        // Also allow clicking directly on the input
        input.on('click', function (e) {
            e.stopPropagation();
        });

        // File selected
        input.on('change', function () {
            handleFileSelect(this, nameDisplay, imageId, allowedExtensions);
        });

        // Drag and drop
        area.on('dragover', function (e) {
            e.preventDefault();
            e.stopPropagation();
            area.addClass('active');
        });

        area.on('dragenter', function (e) {
            e.preventDefault();
            e.stopPropagation();
            area.addClass('active');
        });

        area.on('dragleave', function (e) {
            e.preventDefault();
            e.stopPropagation();
            area.removeClass('active');
        });

        area.on('drop', function (e) {
            e.preventDefault();
            e.stopPropagation();
            area.removeClass('active');
            const files = e.originalEvent.dataTransfer.files;
            if (files.length > 0) {
                input[0].files = files;
                input.trigger('change');
            }
        });
    }

    function handleFileSelect(fileInput, nameDisplay, imageId, allowedExtensions = []) {
        const file = fileInput.files[0];
        if (file) {
            // Validate file extension if allowed extensions are specified
            if (allowedExtensions.length > 0) {
                const fileName = file.name.toLowerCase();
                const fileExtension = fileName.split('.').pop();

                if (!allowedExtensions.includes(fileExtension)) {
                    const allowedTypes = allowedExtensions.join(', ').toUpperCase();
                    alert(`Invalid file type! Only ${allowedTypes} files are allowed. Please select a valid file.`);
                    fileInput.value = '';
                    nameDisplay.text('');
                    return;
                }
            }

            // Validate file size
            const maxSize = 5 * 1024 * 1024; // 5MB
            if (file.size > maxSize) {
                alert('File size must be less than 5MB');
                fileInput.value = '';
                nameDisplay.text('');
                return;
            }

            nameDisplay.text(`✓ ${file.name}`);

            if (imageId) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    $(`#${imageId}`).attr('src', e.target.result).show();
                };
                reader.readAsDataURL(file);
            }
        }
    }

    // Camera functionality
    let stream;

    $('#startCameraBtn').on('click', function (e) {
        e.preventDefault();
        navigator.mediaDevices.getUserMedia({ video: true })
            .then(function (mediaStream) {
                stream = mediaStream;
                const video = document.getElementById('cameraFeed');
                video.srcObject = stream;
                video.play();
                video.style.display = 'block';

                $('#startCameraBtn').hide();
                $('#stopCameraBtn').show();
                $('#capturePhotoBtn').show();
            })
            .catch(function (err) {
                alert('Unable to access camera: ' + err.message);
            });
    });

    $('#stopCameraBtn').on('click', function (e) {
        e.preventDefault();
        if (stream) {
            stream.getTracks().forEach(track => track.stop());
            $('#cameraFeed').hide();
            $('#startCameraBtn').show();
            $('#stopCameraBtn').hide();
            $('#capturePhotoBtn').hide();
        }
    });

    $('#capturePhotoBtn').on('click', function (e) {
        e.preventDefault();
        const video = document.getElementById('cameraFeed');
        const canvas = document.getElementById('captureCanvas');
        const context = canvas.getContext('2d');

        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        context.drawImage(video, 0, 0);

        // 1. Get the compressed Base64 string (e.g., max 500 KB)
        const maxFileSizeKB = 500;
        var base64String = getCompressedBase64(canvas, maxFileSizeKB);

        // 2. Convert Base64 to a File object using our helper function
        const file = dataURLtoFile(base64String, 'camera-photo.jpg');

        // 3. Assign the file to the input element
        const dataTransfer = new DataTransfer();
        dataTransfer.items.add(file);
        document.getElementById('profileUpload').files = dataTransfer.files;

        // 4. Update UI (Same as before)
        $('#profileImage').attr('src', base64String).show();
        $('#profileUploadName').text('✓ camera-photo.jpg');

        // 5. Stop Camera (Same as before)
        if (stream) {
            stream.getTracks().forEach(track => track.stop());
            $('#cameraFeed').hide();
            $('#startCameraBtn').show();
            $('#stopCameraBtn').hide();
            $('#capturePhotoBtn').hide();
        }
    });

    // Form reset handler
    const resetButton = form.querySelector('button[type="reset"]');
    if (resetButton) {
        resetButton.addEventListener('click', function () {
            // Clear file inputs and their display
            $('#class12Marksheet').val('');
            $('#class12Name').text('');

            $('#class10Marksheet').val('');
            $('#class10Name').text('');

            $('#profileUpload').val('');
            $('#profileUploadName').text('');
            $('#profileImage').hide().attr('src', '');

            // Clear invalid states
            $('.form-control, .form-select').removeClass('is-invalid');
            $('.invalid-feedback').removeClass('show');

            // Clear alternate address if synced
            if ($('#sameAsPermenant').is(':checked')) {
                $('#sameAsPermenant').prop('checked', false);
                $('#alternateAddress').prop('readonly', false).css('background-color', '');
                $('#alternateAddress').val('');
            }

            // Stop camera if running
            if (stream) {
                stream.getTracks().forEach(track => track.stop());
                $('#cameraFeed').hide();
                $('#startCameraBtn').show();
                $('#stopCameraBtn').hide();
                $('#capturePhotoBtn').hide();
            }
        });
    }

    // Form validation on submit
    form.addEventListener('submit', function (event) {
        event.preventDefault();

        let isValid = true;

        // Reset invalid states
        $('.form-control, .form-select').removeClass('is-invalid');
        $('.invalid-feedback').removeClass('show');

        // Validate all required fields
        const requiredFields = form.querySelectorAll('[required]');
        requiredFields.forEach(field => {
            const $field = $(field);
            let fieldValid = true;
            let fieldValue = $field.val().trim();

            // Check if field is empty
            if (fieldValue === '') {
                fieldValid = false;
            }

            // Special validation for name fields (max 50 characters)
            if (($field.attr('id') === 'firstName' || $field.attr('id') === 'lastName') && fieldValue !== '') {
                if (fieldValue.length > 50) {
                    fieldValid = false;
                }
            }

            // Special validation for email fields
            if ($field.attr('type') === 'email' && fieldValue !== '') {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(fieldValue)) {
                    fieldValid = false;
                }
            }

            // Special validation for phone fields
            if ($field.attr('type') === 'tel' && fieldValue !== '') {
                const phoneRegex = /^[0-9]{10}$/;
                if (!phoneRegex.test(fieldValue)) {
                    fieldValid = false;
                }
            }

            // Special validation for date fields (DOB - minimum 18 years)
            if ($field.attr('type') === 'date' && $field.attr('id') === 'dob' && fieldValue !== '') {
                const dob = new Date(fieldValue);
                const today = new Date();
                let age = today.getFullYear() - dob.getFullYear();
                const monthDiff = today.getMonth() - dob.getMonth();

                if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) {
                    age--;
                }

                if (age < 18) {
                    fieldValid = false;
                }
            }

            // Special validation for other date fields
            if ($field.attr('type') === 'date' && $field.attr('id') !== 'dob' && fieldValue !== '') {
                if (!fieldValue) {
                    fieldValid = false;
                }
            }

            // Special validation for file inputs
            if ($field.attr('type') === 'file') {
                if (!$field[0].files || $field[0].files.length === 0) {
                    fieldValid = false;
                }
            }

            // Special validation for checkbox
            if ($field.attr('type') === 'checkbox') {
                if (!$field.is(':checked')) {
                    fieldValid = false;
                }
            }

            if (!fieldValid) {
                $field.addClass('is-invalid');
                // Show the corresponding invalid-feedback
                $field.next('.invalid-feedback').addClass('show');
                // For checkboxes and special cases
                $field.closest('.terms-checkbox').find('.invalid-feedback').addClass('show');
                isValid = false;
            }
        });

        // Validate passwords match
        const password = $('#password').val();
        const confirmPassword = $('#confirmPassword').val();
        if (password !== '' && confirmPassword !== '') {
            if (password !== confirmPassword) {
                $('#confirmPassword').addClass('is-invalid');
                $('#confirmPasswordFeedback').addClass('show');
                isValid = false;
            }
        }

        // Validate terms checkbox
        if (!$('#termsCheckbox').is(':checked')) {
            $('#termsCheckbox').addClass('is-invalid');
            $('.terms-checkbox .invalid-feedback').addClass('show');
            isValid = false;
        }

        // If form is valid, submit it
        if (isValid) {
            form.submit();
        } else {
            // Scroll to the first invalid field
            const firstInvalidField = form.querySelector('.is-invalid');
            if (firstInvalidField) {
                firstInvalidField.scrollIntoView({ behavior: 'smooth', block: 'center' });
                firstInvalidField.focus();
            }
        }
    }, false);
});
//Seperate helper function for the base64 string to image file
function dataURLtoFile(dataurl, filename) {
    var arr = dataurl.split(','),
        mime = arr[0].match(/:(.*?);/)[1],
        bstr = atob(arr[1]),
        n = bstr.length,
        u8arr = new Uint8Array(n);

    while (n--) {
        u8arr[n] = bstr.charCodeAt(n);
    }

    return new File([u8arr], filename, { type: mime });
}
//Seperate helper function to get compressed base64 string
function getCompressedBase64(canvas, maxSizeKB) {
    let quality = 0.9; // Start at 90% quality
    let base64String = canvas.toDataURL('image/jpeg', quality);

    // Helper to calculate approximate Base64 size in KB
    // Formula: (Length * 3/4) / 1024 gives approximate Kilobytes
    const getFileSizeKB = (str) => Math.round((str.length * 3 / 4) / 1024);

    let fileSizeKB = getFileSizeKB(base64String);

    // Loop to reduce quality by 10% until it's under the maxSizeKB
    // We stop at quality > 0.1 so we don't end up with an unrecognizable blur
    while (fileSizeKB > maxSizeKB && quality > 0.1) {
        quality -= 0.1;
        base64String = canvas.toDataURL('image/jpeg', quality);
        fileSizeKB = getFileSizeKB(base64String);
    }

    console.log(`Compressed to ~${fileSizeKB}KB (Quality: ${quality.toFixed(1)})`);
    return base64String;
}
