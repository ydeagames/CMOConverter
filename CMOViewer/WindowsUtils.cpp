#include "pch.h"
#include "WindowsUtils.h"
#include "StringCast.h"

namespace WindowsUtils
{
	// �ۑ��_�C�A���O
	bool SaveDialog(const std::string& extension, const std::string& extensionDesc, std::string& result)
	{
		// �t�B���^�[�������`
		std::wstringstream filter;
		std::wstring wextension = string_cast<std::wstring>(extension);
		filter << string_cast<std::wstring>(extensionDesc) << L" (*." << wextension << L")" << std::ends;
		filter << L"*." << wextension << std::ends;
		filter << L"All Files" << L" (*." << L"*" << L")" << std::ends;
		filter << L"*." << L"*" << std::ends;
		filter << std::ends << std::ends;
		std::wstring filterstr = filter.str();

		// �Y���ƃf�t�H���g�t�@�C�����ɕςȕ����񂪕\�������
		wchar_t filename[256] = L"\0";

		// �\���̂�0�ŃN���A
		// �t�@�C���_�C�A���O�ݒ�
		OPENFILENAMEW ofn = { 0 };
		ofn.lStructSize = sizeof(OPENFILENAMEW);
		ofn.lpstrFilter = filterstr.c_str();
		ofn.lpstrFile = filename;
		ofn.nMaxFile = sizeof(filename);
		ofn.Flags = OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT;
		ofn.lpstrDefExt = wextension.c_str();

		// �_�C�A���O�\��
		bool b = (GetSaveFileNameW(&ofn) == TRUE);
		result = string_cast<std::string>(std::wstring(filename));
		return b;
	}

	// �J���_�C�A���O
	bool OpenDialog(const std::string& extension, const std::string& extensionDesc, std::string& result)
	{
		// �t�B���^�[�������`
		std::wstringstream filter;
		std::wstring wextension = string_cast<std::wstring>(extension);
		filter << string_cast<std::wstring>(extensionDesc) << L" (*." << wextension << L")" << std::ends;
		filter << L"*." << wextension << std::ends;
		filter << L"All Files" << L" (*." << L"*" << L")" << std::ends;
		filter << L"*." << L"*" << std::ends;
		filter << std::ends << std::ends;
		std::wstring filterstr = filter.str();

		// �Y���ƃf�t�H���g�t�@�C�����ɕςȕ����񂪕\�������
		wchar_t filename[256] = L"\0";

		// �\���̂�0�ŃN���A
		// �t�@�C���_�C�A���O�ݒ�
		OPENFILENAMEW ofn = { 0 };
		ofn.lStructSize = sizeof(OPENFILENAMEW);
		ofn.lpstrFilter = filterstr.c_str();
		ofn.lpstrFile = filename;
		ofn.nMaxFile = sizeof(filename);
		ofn.Flags = OFN_FILEMUSTEXIST | OFN_HIDEREADONLY;
		ofn.lpstrDefExt = wextension.c_str();

		// �_�C�A���O�\��
		bool b = (GetOpenFileNameW(&ofn) == TRUE);
		result = string_cast<std::string>(std::wstring(filename));
		return b;
	}

	// �f�B���N�g���p�X���o��
	std::string GetDirPath(const std::string& name)
	{
		// �f�B���N�g���̃p�X������
		// �s���I�h���f�B���N�g�����ɓ����Ă��邱�Ƃ�����̂Ő�ɏ���
		const size_t last_slash_idx = name.find_last_of("\\/");
		if (std::string::npos != last_slash_idx)
		{
			return name.substr(0, last_slash_idx + 1);
		}

		return "";
	}

	// �t�@�C���l�[�����o��
	std::string GetFileName(const std::string& name, const std::string& extension)
	{
		std::string filename = name;

		// �f�B���N�g���̃p�X������
		// �s���I�h���f�B���N�g�����ɓ����Ă��邱�Ƃ�����̂Ő�ɏ���
		const size_t last_slash_idx = filename.find_last_of("\\/");
		if (std::string::npos != last_slash_idx)
		{
			filename.erase(0, last_slash_idx + 1);
		}

		// �g���q������Ύ�菜��
		{
			const size_t period_idx = filename.rfind("." + extension);
			if (std::string::npos != period_idx)
			{
				filename.erase(period_idx);
			}
		}

		return filename;
	}
}