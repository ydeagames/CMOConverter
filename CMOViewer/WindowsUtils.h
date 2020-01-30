#pragma once

namespace WindowsUtils
{
	// �ۑ��_�C�A���O
	bool SaveDialog(const std::string& extension, const std::string& extensionDesc, std::string& result);
	// �J���_�C�A���O
	bool OpenDialog(const std::string& extension, const std::string& extensionDesc, std::string& result);
	// �f�B���N�g���p�X���o��
	std::string GetDirPath(const std::string& name);
	// �t�@�C���l�[�����o��
	std::string GetFileName(const std::string& name, const std::string& extension);
}

